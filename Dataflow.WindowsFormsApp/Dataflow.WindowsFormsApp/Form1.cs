using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using System.Windows.Forms;

namespace Dataflow.WindowsFormsApp
{
    public partial class Form1 : Form
    {
        // The head of the dataflow network
        private ITargetBlock<string> _headBlock;

        // Enables the user interface to signal cancellation to the network.
        private CancellationTokenSource _cancellationTokenSource;

        public Form1()
        {
            InitializeComponent();
            toolStripButton1.Click += ToolStripButton1_Click;
            toolStripButton2.Click += ToolStripButton2_Click;
        }

        private void ToolStripButton1_Click(object sender, System.EventArgs e)
        {
            // Create a FolderBrowserDialog object to enable the user to
            // select a folder.
            FolderBrowserDialog dlg = new FolderBrowserDialog
            {
                ShowNewFolderButton = false
            };

            // Set the selected path to the common Sample Pictures folder
            // if it exists.
            string initialDirectory = Path.Combine(
               Environment.GetFolderPath(Environment.SpecialFolder.CommonPictures),
               "Sample Pictures");
            if (Directory.Exists(initialDirectory))
            {
                dlg.SelectedPath = initialDirectory;
            }

            // Show the dialog and process the dataflow network.
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                // Create a new CancellationTokenSource object to enable
                // cancellation.
                _cancellationTokenSource = new CancellationTokenSource();

                // Create the image processing network if needed.
                _headBlock ??= CreateImageProcessingNetwork();

                // Post the selected path to the network.
                _headBlock.Post(dlg.SelectedPath);

                // Enable the Cancel button and disable the Choose Folder button.
                toolStripButton1.Enabled = false;
                toolStripButton2.Enabled = true;

                // Show a wait cursor.
                Cursor = Cursors.WaitCursor;
            }
        }

        private void ToolStripButton2_Click(object sender, EventArgs e)
        {
            _cancellationTokenSource.Cancel();
        }

        private ITargetBlock<string> CreateImageProcessingNetwork()
        {
            var loadBitmapsBlock = new TransformBlock<string, IEnumerable<Bitmap>>(path =>
            {
                try
                {
                    return LoadBitmaps(path);
                }
                catch (OperationCanceledException)
                {
                    return Enumerable.Empty<Bitmap>();
                }
            });

            var createCompositeBitmapBlock = new TransformBlock<IEnumerable<Bitmap>, Bitmap>(bitmaps =>
            {
                try
                {
                    return CreateCompositeBitmap(bitmaps);
                }
                catch (OperationCanceledException)
                {
                    return null;
                }
            });

            var displayCompositeBitmapBlock = new ActionBlock<Bitmap>(bitmap =>
            {
                pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
                pictureBox1.Image = bitmap;

                toolStripButton1.Enabled = true;
                toolStripButton2.Enabled = false;
                Cursor = DefaultCursor;
            },
            new ExecutionDataflowBlockOptions
            {
                TaskScheduler = TaskScheduler.FromCurrentSynchronizationContext()
            });

            var operationCancelledBlock = new ActionBlock<object>(delegate
            {
                pictureBox1.SizeMode = PictureBoxSizeMode.CenterImage;
                pictureBox1.Image = pictureBox1.ErrorImage;

                toolStripButton1.Enabled = true;
                toolStripButton2.Enabled = false;
                Cursor = DefaultCursor;
            },
            new ExecutionDataflowBlockOptions
            {
                TaskScheduler = TaskScheduler.FromCurrentSynchronizationContext()
            });

            //
            // Connect the network.
            //

            // Link loadBitmaps to createCompositeBitmap.
            // The provided predicate ensures that createCompositeBitmap accepts the
            // collection of bitmaps only if that collection has at least one member.
            loadBitmapsBlock.LinkTo(createCompositeBitmapBlock, bitmaps => bitmaps.Count() > 0);
            createCompositeBitmapBlock.LinkTo(displayCompositeBitmapBlock, bitmap => bitmap != null);

            loadBitmapsBlock.LinkTo(operationCancelledBlock);
            createCompositeBitmapBlock.LinkTo(operationCancelledBlock);

            return loadBitmapsBlock;
        }

        private IEnumerable<Bitmap> LoadBitmaps(string path)
        {
            var bitmaps = new List<Bitmap>();

            foreach (string bitmapType in new string[] { "*.bmp", "*.gif", "*.jpg", "*.png", "*.tif" })
            {
                foreach (string fileName in Directory.GetFiles(path, bitmapType))
                {
                    _cancellationTokenSource.Token.ThrowIfCancellationRequested();

                    try
                    {
                        bitmaps.Add(new Bitmap(fileName));
                    }
                    catch
                    { 
                    }
                }
            }

            return bitmaps;
        }

        private Bitmap CreateCompositeBitmap(IEnumerable<Bitmap> bitmaps)
        {
            var bitmapArray = bitmaps.ToArray();

            var largestRectangle = new Rectangle();
            foreach (var bitmap in bitmapArray)
            {
                largestRectangle.Width = bitmap.Width > largestRectangle.Width
                    ? bitmap.Width
                    : largestRectangle.Width;

                largestRectangle.Height = bitmap.Height > largestRectangle.Height
                    ? bitmap.Height
                    : largestRectangle.Height;
            }

            var result = new Bitmap(largestRectangle.Width, largestRectangle.Height, PixelFormat.Format32bppArgb);

            var resultBitmapData = result.LockBits(
                new Rectangle(new Point(), result.Size),
                ImageLockMode.WriteOnly,
                result.PixelFormat);

            var bitmapDataList = (from bitmap in bitmapArray
                                  select bitmap.LockBits(
                                    new Rectangle(new Point(), bitmap.Size),
                                    ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb))
                       .ToList();

            // Compute each column in parallel.
            Parallel.For(0, largestRectangle.Width, new ParallelOptions
            {
                CancellationToken = _cancellationTokenSource.Token
            },
            i =>
            {
                // Compute each row.
                for (int j = 0; j < largestRectangle.Height; j++)
                {
                    // Counts the number of bitmaps whose dimensions
                    // contain the current location.
                    int count = 0;

                    // The sum of all alpha, red, green, and blue components.
                    int a = 0, r = 0, g = 0, b = 0;

                    // For each bitmap, compute the sum of all color components.
                    foreach (var bitmapData in bitmapDataList)
                    {
                        // Ensure that we stay within the bounds of the image.
                        if (bitmapData.Width > i && bitmapData.Height > j)
                        {
                            unsafe
                            {
                                byte* row = (byte*)(bitmapData.Scan0 + (j * bitmapData.Stride));
                                byte* pix = (byte*)(row + (4 * i));
                                a += *pix; pix++;
                                r += *pix; pix++;
                                g += *pix; pix++;
                                b += *pix;
                            }
                            count++;
                        }
                    }

                    //prevent divide by zero in bottom right pixelless corner
                    if (count == 0)
                        break;

                    unsafe
                    {
                        // Compute the average of each color component.
                        a /= count;
                        r /= count;
                        g /= count;
                        b /= count;

                        // Set the result pixel.
                        byte* row = (byte*)(resultBitmapData.Scan0 + (j * resultBitmapData.Stride));
                        byte* pix = (byte*)(row + (4 * i));
                        *pix = (byte)a; pix++;
                        *pix = (byte)r; pix++;
                        *pix = (byte)g; pix++;
                        *pix = (byte)b;
                    }
                }
            });

            // Unlock the source bitmaps.
            for (int i = 0; i < bitmapArray.Length; i++)
            {
                bitmapArray[i].UnlockBits(bitmapDataList[i]);
            }

            // Unlock the result bitmap.
            result.UnlockBits(resultBitmapData);

            return result;
        }
    }
}

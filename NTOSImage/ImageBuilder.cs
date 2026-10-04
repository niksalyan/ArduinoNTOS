using NTOSImage.Models;

namespace NTOSImage
{


    public partial class ImageBuilder : UserControl
    {
        private InputModel input = new InputModel();
        public ImageBuilder()
        {
            InitializeComponent();
            propertyGrid.SelectedObject = input;
        }

        private void propertyGrid_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            try
            {
                pictureBox.Image = new Libs.NTOSBitmap(input).ToBitmap();
            }
            catch
            {
                pictureBox.Image = null;
            }
        }

        private void saveButton_Click(object sender, EventArgs e)
        {
            try
            {
                using SaveFileDialog dialog = new SaveFileDialog
                {
                    Title = "Save NTOS Image",
                    Filter = "NTOS Image (*.nti)|*.nti",
                    DefaultExt = "nti",
                    AddExtension = true,
                    FileName = "image.nti"
                };

                if (dialog.ShowDialog() != DialogResult.OK)
                    return;

                byte[] data = new Libs.NTOSBitmap(input).ToByteArray();

                File.WriteAllBytes(dialog.FileName, data);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.Message,
                    "NTOS Image",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }
    }
}

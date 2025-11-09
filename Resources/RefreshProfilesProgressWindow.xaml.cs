using System.Windows;
using System.Windows.Controls;

namespace ShaderGlass
{
    public partial class RefreshProfilesProgressWindow : UserControl
    {
        public RefreshProfilesProgressWindow()
        {
            InitializeComponent();
        }

        public void UpdateProgress(int current, int total, string status = null)
        {
            if (total > 0)
            {
                double percentage = (double)current / total * 100;
                ProgressBar.Value = percentage;
                ProgressText.Text = $"{current} / {total} ({percentage:F0}%)";
            }
            else
            {
                ProgressBar.Value = 0;
                ProgressText.Text = "0%";
            }

            if (!string.IsNullOrEmpty(status))
            {
                StatusText.Text = status;
            }
        }

        public void SetStatus(string status)
        {
            StatusText.Text = status;
        }
    }
}


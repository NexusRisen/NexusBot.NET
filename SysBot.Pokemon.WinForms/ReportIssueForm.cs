using System;
using System.Drawing;
using System.Windows.Forms;

namespace SysBot.Pokemon.WinForms
{
    public class ReportIssueForm : Form
    {
        private TextBox tbTitle;
        private TextBox tbDescription;
        private Button btnSubmit;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Label lblDescription;

        public ReportIssueForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Report an Issue";
            this.Size = new Size(400, 350);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            lblTitle = new System.Windows.Forms.Label { Text = "Title:", Location = new Point(10, 10), AutoSize = true };
            tbTitle = new TextBox { Location = new Point(10, 30), Width = 360 };

            lblDescription = new System.Windows.Forms.Label { Text = "Description:", Location = new Point(10, 60), AutoSize = true };
            tbDescription = new TextBox { Location = new Point(10, 80), Width = 360, Height = 180, Multiline = true };

            btnSubmit = new Button { Text = "Submit", Location = new Point(270, 275), Width = 100, Height = 30 };
            btnSubmit.Click += BtnSubmit_Click;

            this.Controls.Add(lblTitle);
            this.Controls.Add(tbTitle);
            this.Controls.Add(lblDescription);
            this.Controls.Add(tbDescription);
            this.Controls.Add(btnSubmit);
        }

        private void BtnSubmit_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(tbTitle.Text) || string.IsNullOrWhiteSpace(tbDescription.Text))
            {
                MessageBox.Show("Please enter both a title and description.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var owner = "NexusRisen";
            var repo = "NexusBot.NET";

            var title = Uri.EscapeDataString(tbTitle.Text);
            var body = Uri.EscapeDataString(tbDescription.Text);
            var url = $"https://github.com/{owner}/{repo}/issues/new?title={title}&body={body}";

            try
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = url,
                    UseShellExecute = true
                });

                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to open browser:\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}

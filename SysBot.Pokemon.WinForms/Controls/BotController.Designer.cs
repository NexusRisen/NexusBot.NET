using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;

namespace SysBot.Pokemon.WinForms
{
    partial class BotController
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                OnSharedTick -= AnimationTimer_Tick;

                // Unsubscribe event handlers to prevent memory leaks
                if (contextMenu != null)
                {
                    contextMenu.Opening -= RcMenuOnOpening;
                }

                // Unsubscribe MouseEnter/MouseLeave handlers from all controls
                foreach (var c in Controls.OfType<Control>())
                {
                    if (c != btnActions)
                    {
                        c.MouseEnter -= BotController_MouseEnter;
                        c.MouseLeave -= BotController_MouseLeave;
                    }
                }

                if (mainPanel != null)
                {
                    foreach (var c in mainPanel.Controls.OfType<Control>())
                    {
                        c.MouseEnter -= BotController_MouseEnter;
                        c.MouseLeave -= BotController_MouseLeave;
                    }
                }

                if (components != null)
                {
                    components.Dispose();
                }
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.mainPanel = new System.Windows.Forms.Panel();
            this.statusIndicator = new System.Windows.Forms.PictureBox();
            this.btnActions = new System.Windows.Forms.Button();
            this.contextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.mainPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.statusIndicator)).BeginInit();
            this.SuspendLayout();

            // BotController
            this.SetStyle(System.Windows.Forms.ControlStyles.AllPaintingInWmPaint |
                         System.Windows.Forms.ControlStyles.UserPaint |
                         System.Windows.Forms.ControlStyles.DoubleBuffer |
                         System.Windows.Forms.ControlStyles.ResizeRedraw |
                         System.Windows.Forms.ControlStyles.OptimizedDoubleBuffer, true);
            this.UpdateStyles();
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Transparent;
            this.Controls.Add(this.mainPanel);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.Margin = new System.Windows.Forms.Padding(0, 0, 0, 10);
            this.Name = "BotController";
            this.Size = new System.Drawing.Size(900, 120);
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.BotController_Paint);
            this.MouseEnter += new System.EventHandler(this.BotController_MouseEnter);
            this.MouseLeave += new System.EventHandler(this.BotController_MouseLeave);

            // Main Panel
            this.mainPanel.Anchor = System.Windows.Forms.AnchorStyles.Top |
                                     System.Windows.Forms.AnchorStyles.Bottom |
                                     System.Windows.Forms.AnchorStyles.Left |
                                     System.Windows.Forms.AnchorStyles.Right;
            this.mainPanel.BackColor = System.Drawing.Color.Transparent;
            this.mainPanel.Controls.Add(this.statusIndicator);
            this.mainPanel.Controls.Add(this.btnActions);
            this.mainPanel.Location = new System.Drawing.Point(3, 3);
            this.mainPanel.Name = "mainPanel";
            this.mainPanel.Size = new System.Drawing.Size(894, 114);
            this.mainPanel.TabIndex = 0;
            this.mainPanel.Paint += new System.Windows.Forms.PaintEventHandler(this.MainPanel_Paint);
            this.mainPanel.MouseEnter += new System.EventHandler(this.BotController_MouseEnter);
            this.mainPanel.MouseLeave += new System.EventHandler(this.BotController_MouseLeave);

            // Status Indicator (pulsing indicator on the left)
            this.statusIndicator.BackColor = System.Drawing.Color.Transparent;
            this.statusIndicator.Location = new System.Drawing.Point(15, 25);
            this.statusIndicator.Name = "statusIndicator";
            this.statusIndicator.Size = new System.Drawing.Size(40, 40);
            this.statusIndicator.TabIndex = 0;
            this.statusIndicator.TabStop = false;
            this.statusIndicator.Paint += new System.Windows.Forms.PaintEventHandler(this.StatusIndicator_Paint);

            // Actions Button (top-right corner)
            this.btnActions.Anchor = System.Windows.Forms.AnchorStyles.Top |
                                    System.Windows.Forms.AnchorStyles.Right;
            this.btnActions.BackColor = System.Drawing.Color.FromArgb(102, 192, 244);
            this.btnActions.FlatAppearance.BorderSize = 0;
            this.btnActions.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(92, 173, 220);
            this.btnActions.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(122, 207, 255);
            this.btnActions.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnActions.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnActions.ForeColor = System.Drawing.Color.White;
            this.btnActions.Location = new System.Drawing.Point(750, 30);
            this.btnActions.Name = "btnActions";
            this.btnActions.Size = new System.Drawing.Size(120, 35);
            this.btnActions.TabIndex = 5;
            this.btnActions.Text = "COMMANDS";
            this.btnActions.UseVisualStyleBackColor = false;
            this.btnActions.Click += new System.EventHandler(this.BtnActions_Click);
            this.btnActions.Paint += new System.Windows.Forms.PaintEventHandler(this.BtnActions_Paint);
            this.btnActions.MouseEnter += new System.EventHandler(this.BtnActions_MouseEnter);
            this.btnActions.MouseLeave += new System.EventHandler(this.BtnActions_MouseLeave);

            // Context Menu
            this.contextMenu.BackColor = SysBot.Pokemon.WinForms.Helpers.WinFormsTheme.SurfaceColor;
            this.contextMenu.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.contextMenu.ForeColor = System.Drawing.Color.White;
            this.contextMenu.Name = "contextMenu";
            this.contextMenu.RenderMode = System.Windows.Forms.ToolStripRenderMode.Professional;
            this.contextMenu.ShowImageMargin = false;
            this.contextMenu.Size = new System.Drawing.Size(150, 4);

            // Component initialization
            this.mainPanel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.statusIndicator)).EndInit();
            this.ResumeLayout(false);
        }

        #endregion

        // Main controls
        private System.Windows.Forms.Panel mainPanel;
        private System.Windows.Forms.PictureBox statusIndicator;
        private System.Windows.Forms.Button btnActions;
        private System.Windows.Forms.ContextMenuStrip contextMenu;
    }
}

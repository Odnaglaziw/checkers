namespace checkers
{
    partial class Main
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            panel1 = new Panel();
            button2 = new Button();
            button1 = new Button();
            flowLayoutPanel1 = new FlowLayoutPanel();
            textBox1 = new TextBox();
            panel1.SuspendLayout();
            SuspendLayout();
            // 
            // panel1
            // 
            panel1.Controls.Add(textBox1);
            panel1.Controls.Add(button2);
            panel1.Controls.Add(button1);
            panel1.Dock = DockStyle.Left;
            panel1.Location = new Point(0, 0);
            panel1.Name = "panel1";
            panel1.Size = new Size(200, 450);
            panel1.TabIndex = 1;
            // 
            // button2
            // 
            button2.Location = new Point(12, 299);
            button2.Name = "button2";
            button2.Size = new Size(182, 43);
            button2.TabIndex = 1;
            button2.Text = "Обновить";
            button2.UseVisualStyleBackColor = true;
            button2.Click += button2_Click;
            // 
            // button1
            // 
            button1.Location = new Point(12, 389);
            button1.Name = "button1";
            button1.Size = new Size(182, 49);
            button1.TabIndex = 0;
            button1.Text = "Создать";
            button1.UseVisualStyleBackColor = true;
            button1.Click += button1_Click;
            // 
            // flowLayoutPanel1
            // 
            flowLayoutPanel1.Dock = DockStyle.Fill;
            flowLayoutPanel1.Location = new Point(200, 0);
            flowLayoutPanel1.Name = "flowLayoutPanel1";
            flowLayoutPanel1.Size = new Size(600, 450);
            flowLayoutPanel1.TabIndex = 2;
            // 
            // textBox1
            // 
            textBox1.Location = new Point(12, 348);
            textBox1.Name = "textBox1";
            textBox1.Size = new Size(182, 35);
            textBox1.TabIndex = 2;
            // 
            // Main
            // 
            AutoScaleDimensions = new SizeF(12F, 30F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(flowLayoutPanel1);
            Controls.Add(panel1);
            Font = new Font("Segoe UI", 15.75F, FontStyle.Regular, GraphicsUnit.Point, 204);
            Name = "Main";
            Text = "Main";
            Load += Main_Load;
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private Panel panel1;
        private FlowLayoutPanel flowLayoutPanel1;
        private Button button1;
        private Button button2;
        private TextBox textBox1;
    }
}
namespace BscanILL.Export.ILL.ArielAddressBookExtractor
{
	partial class ArielAddressBookExtractorForm
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
			this.label1 = new System.Windows.Forms.Label();
			this.comboBoxVersion = new System.Windows.Forms.ComboBox();
			this.buttonExtract = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(34, 28);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(68, 13);
			this.label1.TabIndex = 0;
			this.label1.Text = "Ariel Version:";
			// 
			// comboBoxVersion
			// 
			this.comboBoxVersion.FormattingEnabled = true;
			this.comboBoxVersion.Items.AddRange(new object[] {
            "4.1.1"});
			this.comboBoxVersion.Location = new System.Drawing.Point(108, 25);
			this.comboBoxVersion.Name = "comboBoxVersion";
			this.comboBoxVersion.Size = new System.Drawing.Size(135, 21);
			this.comboBoxVersion.TabIndex = 1;
			// 
			// buttonExtract
			// 
			this.buttonExtract.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
			this.buttonExtract.Location = new System.Drawing.Point(95, 84);
			this.buttonExtract.Name = "buttonExtract";
			this.buttonExtract.Size = new System.Drawing.Size(98, 23);
			this.buttonExtract.TabIndex = 2;
			this.buttonExtract.Text = "Extract!";
			this.buttonExtract.UseVisualStyleBackColor = true;
			this.buttonExtract.Click += new System.EventHandler(this.Extract_Click);
			// 
			// ArielAddressBookExtractorForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(289, 119);
			this.Controls.Add(this.buttonExtract);
			this.Controls.Add(this.comboBoxVersion);
			this.Controls.Add(this.label1);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.Name = "ArielAddressBookExtractorForm";
			this.Text = "Address Book Extractor";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.ComboBox comboBoxVersion;
		private System.Windows.Forms.Button buttonExtract;
	}
}


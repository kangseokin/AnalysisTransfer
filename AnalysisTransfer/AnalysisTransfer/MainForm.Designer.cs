namespace AnalysisTransfer
{
    partial class MainForm
    {
        /// <summary>
        /// 필수 디자이너 변수입니다.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 사용 중인 모든 리소스를 정리합니다.
        /// </summary>
        /// <param name="disposing">관리되는 리소스를 삭제해야 하면 true이고, 그렇지 않으면 false입니다.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form 디자이너에서 생성한 코드

        /// <summary>
        /// 디자이너 지원에 필요한 메서드입니다. 
        /// 이 메서드의 내용을 코드 편집기로 수정하지 마세요.
        /// </summary>
        private void InitializeComponent()
        {
            this.transmissionDisplay1 = new TransmissionDisplay.TransmissionDisplay();
            this.transmissionDataList1 = new TransmissionDataList.TransmissionDataList();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.btProductionInformation1 = new BTProductionInformation.BTProductionInformation();
            this.SuspendLayout();
            // 
            // transmissionDisplay1
            // 
            this.transmissionDisplay1.Dock = System.Windows.Forms.DockStyle.Top;
            this.transmissionDisplay1.Location = new System.Drawing.Point(0, 0);
            this.transmissionDisplay1.Margin = new System.Windows.Forms.Padding(5);
            this.transmissionDisplay1.Name = "transmissionDisplay1";
            this.transmissionDisplay1.Padding = new System.Windows.Forms.Padding(3);
            this.transmissionDisplay1.Size = new System.Drawing.Size(1403, 377);
            this.transmissionDisplay1.TabIndex = 0;
            // 
            // transmissionDataList1
            // 
            this.transmissionDataList1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.transmissionDataList1.Location = new System.Drawing.Point(0, 377);
            this.transmissionDataList1.Name = "transmissionDataList1";
            this.transmissionDataList1.Padding = new System.Windows.Forms.Padding(3);
            this.transmissionDataList1.Size = new System.Drawing.Size(1403, 279);
            this.transmissionDataList1.TabIndex = 1;
            // 
            // statusStrip1
            // 
            this.statusStrip1.Location = new System.Drawing.Point(0, 924);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(1403, 22);
            this.statusStrip1.TabIndex = 2;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // btProductionInformation1
            // 
            this.btProductionInformation1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.btProductionInformation1.Location = new System.Drawing.Point(0, 656);
            this.btProductionInformation1.Name = "btProductionInformation1";
            this.btProductionInformation1.Padding = new System.Windows.Forms.Padding(3);
            this.btProductionInformation1.Size = new System.Drawing.Size(1403, 268);
            this.btProductionInformation1.TabIndex = 3;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1403, 946);
            this.Controls.Add(this.transmissionDataList1);
            this.Controls.Add(this.btProductionInformation1);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.transmissionDisplay1);
            this.Name = "MainForm";
            this.Text = "Form1";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private TransmissionDisplay.TransmissionDisplay transmissionDisplay1;
        private TransmissionDataList.TransmissionDataList transmissionDataList1;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private BTProductionInformation.BTProductionInformation btProductionInformation1;
    }
}


namespace power4000
{
    partial class Form1
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle21 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle22 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle23 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle24 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle25 = new System.Windows.Forms.DataGridViewCellStyle();
            this.btn_start = new System.Windows.Forms.Button();
            this.btn_end = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.txtip = new System.Windows.Forms.TextBox();
            this.txtport = new System.Windows.Forms.TextBox();
            this.Dgv = new System.Windows.Forms.DataGridView();
            this.panel1 = new System.Windows.Forms.Panel();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.col_Seq = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.col_Time = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.col_Type = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.col_Mid = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.col_Msg = new System.Windows.Forms.DataGridViewTextBoxColumn();
            ((System.ComponentModel.ISupportInitialize)(this.Dgv)).BeginInit();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // btn_start
            // 
            this.btn_start.Location = new System.Drawing.Point(351, 82);
            this.btn_start.Name = "btn_start";
            this.btn_start.Size = new System.Drawing.Size(75, 23);
            this.btn_start.TabIndex = 0;
            this.btn_start.Text = "start";
            this.btn_start.UseVisualStyleBackColor = true;
            this.btn_start.Click += new System.EventHandler(this.btn_start_Click);
            // 
            // btn_end
            // 
            this.btn_end.Location = new System.Drawing.Point(443, 82);
            this.btn_end.Name = "btn_end";
            this.btn_end.Size = new System.Drawing.Size(75, 23);
            this.btn_end.TabIndex = 1;
            this.btn_end.Text = "End";
            this.btn_end.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 86);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(16, 12);
            this.label1.TabIndex = 2;
            this.label1.Text = "IP";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(168, 86);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(27, 12);
            this.label2.TabIndex = 3;
            this.label2.Text = "Port";
            // 
            // txtip
            // 
            this.txtip.Location = new System.Drawing.Point(34, 83);
            this.txtip.Name = "txtip";
            this.txtip.Size = new System.Drawing.Size(100, 21);
            this.txtip.TabIndex = 4;
            // 
            // txtport
            // 
            this.txtport.Location = new System.Drawing.Point(201, 83);
            this.txtport.Name = "txtport";
            this.txtport.Size = new System.Drawing.Size(100, 21);
            this.txtport.TabIndex = 5;
            // 
            // Dgv
            // 
            this.Dgv.BackgroundColor = System.Drawing.SystemColors.Control;
            this.Dgv.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.Dgv.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.col_Seq,
            this.col_Time,
            this.col_Type,
            this.col_Mid,
            this.col_Msg});
            this.Dgv.GridColor = System.Drawing.SystemColors.Control;
            this.Dgv.Location = new System.Drawing.Point(12, 317);
            this.Dgv.Name = "Dgv";
            this.Dgv.ReadOnly = true;
            this.Dgv.RowTemplate.Height = 23;
            this.Dgv.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.Dgv.Size = new System.Drawing.Size(826, 221);
            this.Dgv.TabIndex = 6;
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.LightSlateGray;
            this.panel1.Controls.Add(this.button2);
            this.panel1.Controls.Add(this.button1);
            this.panel1.Controls.Add(this.label5);
            this.panel1.Location = new System.Drawing.Point(4, 6);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(842, 36);
            this.panel1.TabIndex = 7;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(10, 68);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(110, 12);
            this.label3.TabIndex = 9;
            this.label3.Text = "Networking Setting";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(12, 302);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(55, 12);
            this.label4.TabIndex = 10;
            this.label4.Text = "Data Log";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Segoe UI", 20F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.label5.Location = new System.Drawing.Point(1, -6);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(601, 37);
            this.label5.TabIndex = 0;
            this.label5.Text = "Tighten PowerFocus / PowerMac Server(v1.0)";
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(809, 1);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(30, 30);
            this.button1.TabIndex = 1;
            this.button1.Text = "button1";
            this.button1.UseVisualStyleBackColor = true;
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(773, 1);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(30, 30);
            this.button2.TabIndex = 2;
            this.button2.Text = "button2";
            this.button2.UseVisualStyleBackColor = true;
            // 
            // col_Seq
            // 
            dataGridViewCellStyle21.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle21.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.col_Seq.DefaultCellStyle = dataGridViewCellStyle21;
            this.col_Seq.HeaderText = "SEQ";
            this.col_Seq.Name = "col_Seq";
            this.col_Seq.ReadOnly = true;
            // 
            // col_Time
            // 
            dataGridViewCellStyle22.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle22.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.col_Time.DefaultCellStyle = dataGridViewCellStyle22;
            this.col_Time.HeaderText = "TIME";
            this.col_Time.Name = "col_Time";
            this.col_Time.ReadOnly = true;
            // 
            // col_Type
            // 
            dataGridViewCellStyle23.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle23.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.col_Type.DefaultCellStyle = dataGridViewCellStyle23;
            this.col_Type.HeaderText = "TYPE";
            this.col_Type.Name = "col_Type";
            this.col_Type.ReadOnly = true;
            // 
            // col_Mid
            // 
            dataGridViewCellStyle24.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle24.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.col_Mid.DefaultCellStyle = dataGridViewCellStyle24;
            this.col_Mid.HeaderText = "MID";
            this.col_Mid.Name = "col_Mid";
            this.col_Mid.ReadOnly = true;
            // 
            // col_Msg
            // 
            this.col_Msg.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            dataGridViewCellStyle25.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle25.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.col_Msg.DefaultCellStyle = dataGridViewCellStyle25;
            this.col_Msg.HeaderText = "Message";
            this.col_Msg.Name = "col_Msg";
            this.col_Msg.ReadOnly = true;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(850, 550);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.btn_end);
            this.Controls.Add(this.btn_start);
            this.Controls.Add(this.txtport);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.Dgv);
            this.Controls.Add(this.txtip);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "Form1";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
            ((System.ComponentModel.ISupportInitialize)(this.Dgv)).EndInit();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btn_start;
        private System.Windows.Forms.Button btn_end;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtip;
        private System.Windows.Forms.TextBox txtport;
        private System.Windows.Forms.DataGridView Dgv;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.DataGridViewTextBoxColumn col_Seq;
        private System.Windows.Forms.DataGridViewTextBoxColumn col_Time;
        private System.Windows.Forms.DataGridViewTextBoxColumn col_Type;
        private System.Windows.Forms.DataGridViewTextBoxColumn col_Mid;
        private System.Windows.Forms.DataGridViewTextBoxColumn col_Msg;
    }
}


﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using CMCS.DumblyConcealer.Win.Core;
using CMCS.Common.Utilities;
using CMCS.DumblyConcealer.Tasks.CarDeviationRoute;
using CMCS.DumblyConcealer.Enums;

namespace CMCS.DumblyConcealer.Win.DumblyTasks
{
    public partial class FrmCarSpeedRoute : TaskForm
    {
        RTxtOutputer rTxtOutputer;
        TaskSimpleScheduler taskSimpleScheduler = new TaskSimpleScheduler();
        Boolean isExeFinish = true;

        public FrmCarSpeedRoute()
        {
            InitializeComponent();
        }

        private void FrmCarSpeedRoute_Load(object sender, EventArgs e)
        {
            this.rTxtOutputer = new RTxtOutputer(rtxtOutput);
            ExecuteAllTask();
        }

        /// <summary>
        /// 执行所有任务
        /// </summary>
        void ExecuteAllTask()
        {
            CarSpeedRouteDAO carDeviationRouteDAO = CarSpeedRouteDAO.GetInstance();
            taskSimpleScheduler.StartNewTask("车速异常监测", () =>
            {
                if (isExeFinish)
                {
                    isExeFinish = false;
                    carDeviationRouteDAO.SaveToCarSpeedRoute(this.rTxtOutputer.Output);
                    isExeFinish = true;
                }
            }, 30*1000, OutputError);
        }

        /// <summary>
        /// 输出异常信息
        /// </summary>
        /// <param name="text"></param>
        /// <param name="ex"></param>
        void OutputError(string text, Exception ex)
        {
            this.isExeFinish = true;
            this.rTxtOutputer.Output(text + Environment.NewLine + ex.Message, eOutputType.Error);
        }

        /// <summary>
        /// 窗体关闭后
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FrmCarSpeedRoute_FormClosed(object sender, FormClosedEventArgs e)
        {
            // 注意：必须取消任务
            this.taskSimpleScheduler.Cancal();
        }
    }
}

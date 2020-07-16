using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using CMCS.CarTransport.DAO;
using CMCS.CarTransport.Weighter.Core;
using CMCS.CarTransport.Weighter.Enums;
using CMCS.CarTransport.Weighter.Frms.Sys;
using CMCS.Common;
using CMCS.Common.DAO;
using CMCS.Common.Entities;
using CMCS.Common.Entities.BaseInfo;
using CMCS.Common.Entities.CarTransport;
using CMCS.Common.Entities.Sys;
using CMCS.Common.Enums;
using CMCS.Common.Utilities;
using CMCS.Common.Views;
using DevComponents.DotNetBar;
using DevComponents.DotNetBar.Controls;
using DevComponents.DotNetBar.SuperGrid;
using HikVisionSDK.Core;
using System.Threading.Tasks;
using System.Drawing.Printing;
using LED.YB_Bx5K1;

namespace CMCS.CarTransport.Weighter.Frms
{
	public partial class FrmWeighter : DevComponents.DotNetBar.Metro.MetroForm
	{
		/// <summary>
		/// ����Ψһ��ʶ��
		/// </summary>
		public static string UniqueKey = "FrmWeighter";

		public FrmWeighter()
		{
			InitializeComponent();
		}

		#region Vars

		CarTransportDAO carTransportDAO = CarTransportDAO.GetInstance();
		WeighterDAO weighterDAO = WeighterDAO.GetInstance();
		CommonDAO commonDAO = CommonDAO.GetInstance();

		/// <summary>
		/// �ȴ��ϴ���ץ��
		/// </summary>
		Queue<string> waitForUpload = new Queue<string>();

		IocControler iocControler;
		/// <summary>
		/// ��������
		/// </summary>
		VoiceSpeaker voiceSpeaker = new VoiceSpeaker();

		bool inductorCoil1 = false;
		/// <summary>
		/// �ظ�1״̬ true=���ź�  false=���ź�
		/// </summary>
		public bool InductorCoil1
		{
			get
			{
				return inductorCoil1;
			}
			set
			{
				if (value && this.CurrentFlowFlag == eFlowFlag.�ȴ�����)
				{
					this.CurrentDirection = eDirection.Way1;
					this.CurrentFlowFlag = eFlowFlag.��ʼ����;
				}
				inductorCoil1 = value;

				panCurrentWeight.Refresh();

				commonDAO.SetSignalDataValue(CommonAppConfig.GetInstance().AppIdentifier, eSignalDataName.�ظ�1�ź�.ToString(), value ? "1" : "0");
			}
		}

		int inductorCoil1Port;
		/// <summary>
		/// �ظ�1�˿�
		/// </summary>
		public int InductorCoil1Port
		{
			get { return inductorCoil1Port; }
			set { inductorCoil1Port = value; }
		}

		bool inductorCoil2 = false;
		/// <summary>
		/// �ظ�2״̬ true=���ź�  false=���ź�
		/// </summary>
		public bool InductorCoil2
		{
			get
			{
				return inductorCoil2;
			}
			set
			{
				inductorCoil2 = value;

				panCurrentWeight.Refresh();

				commonDAO.SetSignalDataValue(CommonAppConfig.GetInstance().AppIdentifier, eSignalDataName.�ظ�2�ź�.ToString(), value ? "1" : "0");
			}
		}

		int inductorCoil2Port;
		/// <summary>
		/// �ظ�2�˿�
		/// </summary>
		public int InductorCoil2Port
		{
			get { return inductorCoil2Port; }
			set { inductorCoil2Port = value; }
		}

		bool infraredSensor1 = false;
		/// <summary>
		/// ����1״̬ true=�ڵ�  false=��ͨ
		/// </summary>
		public bool InfraredSensor1
		{
			get
			{
				return infraredSensor1;
			}
			set
			{
				infraredSensor1 = value;

				panCurrentWeight.Refresh();

				commonDAO.SetSignalDataValue(CommonAppConfig.GetInstance().AppIdentifier, eSignalDataName.����1�ź�.ToString(), value ? "1" : "0");
			}
		}

		int infraredSensor1Port;
		/// <summary>
		/// ����1�˿�
		/// </summary>
		public int InfraredSensor1Port
		{
			get { return infraredSensor1Port; }
			set { infraredSensor1Port = value; }
		}

		bool infraredSensor2 = false;
		/// <summary>
		/// ����2״̬ true=�ڵ�  false=��ͨ
		/// </summary>
		public bool InfraredSensor2
		{
			get
			{
				return infraredSensor2;
			}
			set
			{
				infraredSensor2 = value;

				panCurrentWeight.Refresh();

				commonDAO.SetSignalDataValue(CommonAppConfig.GetInstance().AppIdentifier, eSignalDataName.����2�ź�.ToString(), value ? "1" : "0");
			}
		}

		int infraredSensor2Port;
		/// <summary>
		/// ����2�˿�
		/// </summary>
		public int InfraredSensor2Port
		{
			get { return infraredSensor2Port; }
			set { infraredSensor2Port = value; }
		}

		bool infraredSensor3 = false;
		/// <summary>
		/// ����3״̬ true=�ڵ�  false=��ͨ
		/// </summary>
		public bool InfraredSensor3
		{
			get
			{
				return infraredSensor3;
			}
			set
			{
				infraredSensor3 = value;

				panCurrentWeight.Refresh();

				commonDAO.SetSignalDataValue(CommonAppConfig.GetInstance().AppIdentifier, eSignalDataName.����3�ź�.ToString(), value ? "1" : "0");
			}
		}

		int infraredSensor3Port;
		/// <summary>
		/// ����3�˿�
		/// </summary>
		public int InfraredSensor3Port
		{
			get { return infraredSensor3Port; }
			set { infraredSensor3Port = value; }
		}

		bool wbSteady = false;
		/// <summary>
		/// �ذ��Ǳ��ȶ�״̬
		/// </summary>
		public bool WbSteady
		{
			get { return wbSteady; }
			set
			{
				wbSteady = value;

				this.panCurrentWeight.Style.ForeColor.Color = (value ? Color.Lime : Color.Red);

				panCurrentWeight.Refresh();

				commonDAO.SetSignalDataValue(CommonAppConfig.GetInstance().AppIdentifier, eSignalDataName.�ذ��Ǳ�_�ȶ�.ToString(), value ? "1" : "0");
			}
		}

		double wbMinWeight = 0;
		/// <summary>
		/// �ذ��Ǳ���С���� ��λ����
		/// </summary>
		public double WbMinWeight
		{
			get { return wbMinWeight; }
			set
			{
				wbMinWeight = value;
			}
		}

		bool autoHandMode = true;
		/// <summary>
		/// �Զ�ģʽ=true  �ֶ�ģʽ=false
		/// </summary>
		public bool AutoHandMode
		{
			get { return autoHandMode; }
			set
			{
				autoHandMode = value;

				btnSelectAutotruck_BuyFuel.Visible = !value;
				btnSelectAutotruck_Goods.Visible = !value;

				btnSaveTransport_BuyFuel.Visible = !value;
				btnSaveTransport_Goods.Visible = !value;

				btnReset_BuyFuel.Visible = !value;
				btnReset_Goods.Visible = !value;
			}
		}

		/// <summary>
		/// ��ǰ���ʶ��ĳ��ƺ�
		/// </summary>
		string CameraCarNumber = string.Empty;

		public static PassCarQueuer passCarQueuer = new PassCarQueuer();

		ImperfectCar currentImperfectCar;
		/// <summary>
		/// ʶ���ѡ��ĳ���ƾ֤
		/// </summary>
		public ImperfectCar CurrentImperfectCar
		{
			get { return currentImperfectCar; }
			set
			{
				currentImperfectCar = value;

				if (value != null)
					panCurrentCarNumber.Text = value.Voucher;
				else
					panCurrentCarNumber.Text = "�ȴ�����";
			}
		}

		string direction = "˫���";

		/// <summary>
		/// �̶��ϰ�����
		/// </summary>
		public string Direction
		{
			get { return direction; }
			set
			{
				direction = value;
				if (value == "���ϰ�")
				{
					slightLED2.Visible = false;
					lab_slightLED2.Visible = false;

					slightRwer2.Visible = false;
					lab_slightRwer2.Visible = false;

					panRightGateControl.Visible = false;
					tableLayoutPanel2.ColumnStyles[3].Width = 0;
					tableLayoutPanel2.Refresh();

					this.CurrentDirection = eDirection.Way1;
				}
				else if (value == "���ϰ�")
				{
					slightLED1.Visible = false;
					lab_slightLED1.Visible = false;

					slightRwer1.Visible = false;
					lab_slightRwer1.Visible = false;

					panLeftGateControl.Visible = false;
					tableLayoutPanel2.ColumnStyles[0].Width = 0;
					tableLayoutPanel2.Refresh();

					this.CurrentDirection = eDirection.Way2;
				}
			}
		}

		eDirection currentDirection;
		/// <summary>
		/// ��ǰ�ϰ�����
		/// </summary>
		public eDirection CurrentDirection
		{
			get { return currentDirection; }
			set { currentDirection = value; }
		}

		eFlowFlag currentFlowFlag = eFlowFlag.�ȴ�����;
		/// <summary>
		/// ��ǰҵ�����̱�ʶ
		/// </summary>
		public eFlowFlag CurrentFlowFlag
		{
			get { return currentFlowFlag; }
			set
			{
				currentFlowFlag = value;
				InvokeEx(() =>
				{
					lblFlowFlag.Text = value.ToString();
				});
			}
		}

		CmcsAutotruck currentAutotruck;
		/// <summary>
		/// ��ǰ��
		/// </summary>
		public CmcsAutotruck CurrentAutotruck
		{
			get { return currentAutotruck; }
			set
			{
				currentAutotruck = value;

				if (value != null)
				{
					commonDAO.SetSignalDataValue(CommonAppConfig.GetInstance().AppIdentifier, eSignalDataName.��ǰ��Id.ToString(), value.Id);
					commonDAO.SetSignalDataValue(CommonAppConfig.GetInstance().AppIdentifier, eSignalDataName.��ǰ����.ToString(), value.CarNumber);

					CmcsEPCCard ePCCard = Dbers.GetInstance().SelfDber.Get<CmcsEPCCard>(value.EPCCardId);
					if (value.CarType == eCarType.�볧ú.ToString())
					{
						if (ePCCard != null) txtTagId_BuyFuel.Text = ePCCard.TagId;

						txtCarNumber_BuyFuel.Text = value.CarNumber;
						superTabControl2.SelectedTab = superTabItem_BuyFuel;
					}
					else if (value.CarType == eCarType.��������.ToString())
					{
						if (ePCCard != null) txtTagId_Goods.Text = ePCCard.TagId;

						txtCarNumber_Goods.Text = value.CarNumber;
						superTabControl2.SelectedTab = superTabItem_Goods;
					}

					panCurrentCarNumber.Text = value.CarNumber;
				}
				else
				{
					commonDAO.SetSignalDataValue(CommonAppConfig.GetInstance().AppIdentifier, eSignalDataName.��ǰ��Id.ToString(), string.Empty);
					commonDAO.SetSignalDataValue(CommonAppConfig.GetInstance().AppIdentifier, eSignalDataName.��ǰ����.ToString(), string.Empty);

					txtCarNumber_BuyFuel.ResetText();
					txtCarNumber_Goods.ResetText();

					txtTagId_BuyFuel.ResetText();
					txtTagId_Goods.ResetText();

					panCurrentCarNumber.ResetText();
				}
			}
		}

		private bool autoPrint = false;
		/// <summary>
		/// �Զ���ӡ����
		/// </summary>
		public bool AutoPrint
		{
			get { return autoPrint; }
			set
			{
				autoPrint = value;
			}
		}

		/// <summary>
		/// �Ƿ����þ��س���
		/// </summary>
		bool IsTicketDiff = false;

		/// <summary>
		/// ���س�����ֵ
		/// </summary>
		decimal TicketDiff = 0;

		#endregion

		/// <summary>
		/// �����ʼ��
		/// </summary>
		private void InitForm()
		{
			FrmDebugConsole.GetInstance();

			// Ĭ���Զ�
			sbtnChangeAutoHandMode.Value = true;
			Direction = commonDAO.GetAppletConfigString("�ϰ�����");
			// ���ó���Զ�̿�������
			commonDAO.ResetAppRemoteControlCmd(CommonAppConfig.GetInstance().AppIdentifier);

			btnRefresh_Click(null, null);
		}

		private void FrmWeighter_Load(object sender, EventArgs e)
		{
		}

		private void FrmWeighter_Shown(object sender, EventArgs e)
		{
			InitHardware();

			InitForm();
		}

		private void FrmQueuer_FormClosing(object sender, FormClosingEventArgs e)
		{
			// ж���豸
			UnloadHardware();
		}

		#region �豸���

		#region IO������

		void Iocer_StatusChange(bool status)
		{
			// �����豸״̬ 
			InvokeEx(() =>
			{
				slightIOC.LightColor = (status ? Color.Green : Color.Red);

				commonDAO.SetSignalDataValue(CommonAppConfig.GetInstance().AppIdentifier, eSignalDataName.IO������_����״̬.ToString(), status ? "1" : "0");
			});
		}

		/// <summary>
		/// IO��������������ʱ����
		/// </summary>
		/// <param name="receiveValue"></param>
		void Iocer_Received(int[] receiveValue)
		{
			// ���յظ�״̬  
			InvokeEx(() =>
			{
				this.InductorCoil1 = (receiveValue[this.InductorCoil1Port - 1] == 1);
				this.InductorCoil2 = (receiveValue[this.InductorCoil2Port - 1] == 1);

				this.InfraredSensor1 = (receiveValue[this.InfraredSensor1Port - 1] == 1);
				this.InfraredSensor2 = (receiveValue[this.InfraredSensor2Port - 1] == 1);
				this.InfraredSensor3 = (receiveValue[this.InfraredSensor3Port - 1] == 1);
			});
		}

		/// <summary>
		/// ǰ������
		/// </summary>
		void FrontGateUp()
		{
			if (this.CurrentImperfectCar == null) return;

			if (this.CurrentImperfectCar.PassWay == eDirection.Way1)
			{
				this.iocControler.Gate2Up();
				this.iocControler.GreenLight2();
			}
			else if (this.CurrentImperfectCar.PassWay == eDirection.Way2)
			{
				this.iocControler.Gate1Up();
				this.iocControler.GreenLight1();
			}
		}

		/// <summary>
		/// ǰ������
		/// </summary>
		void FrontGateDown()
		{
			if (this.CurrentImperfectCar == null) return;

			if (this.CurrentImperfectCar.PassWay == eDirection.Way1)
			{
				this.iocControler.Gate2Down();
				this.iocControler.RedLight2();
			}
			else if (this.CurrentImperfectCar.PassWay == eDirection.Way2)
			{
				this.iocControler.Gate1Down();
				this.iocControler.RedLight1();
			}
		}

		/// <summary>
		/// ������
		/// </summary>
		void BackGateUp()
		{
			if (this.CurrentImperfectCar == null) return;

			if (this.CurrentImperfectCar.PassWay == eDirection.Way1)
			{
				this.iocControler.Gate1Up();
				this.iocControler.GreenLight1();
			}
			else if (this.CurrentImperfectCar.PassWay == eDirection.Way2)
			{
				this.iocControler.Gate2Up();
				this.iocControler.GreenLight2();
			}
		}

		/// <summary>
		/// �󷽽���
		/// </summary>
		void BackGateDown()
		{
			if (this.CurrentImperfectCar == null) return;

			if (this.CurrentImperfectCar.PassWay == eDirection.Way1)
			{
				this.iocControler.Gate1Down();
				this.iocControler.RedLight1();
			}
			else if (this.CurrentImperfectCar.PassWay == eDirection.Way2)
			{
				this.iocControler.Gate2Down();
				this.iocControler.RedLight2();
			}
		}

		#endregion

		#region ������

		void Rwer1_OnScanError(Exception ex)
		{
			Log4Neter.Error("������1", ex);
		}

		void Rwer1_OnStatusChange(bool status)
		{
			// �����豸״̬ 
			InvokeEx(() =>
			{
				slightRwer1.LightColor = (status ? Color.Green : Color.Red);

				commonDAO.SetSignalDataValue(CommonAppConfig.GetInstance().AppIdentifier, eSignalDataName.������1_����״̬.ToString(), status ? "1" : "0");
			});
		}

		void Rwer2_OnScanError(Exception ex)
		{
			Log4Neter.Error("������2", ex);
		}

		void Rwer2_OnStatusChange(bool status)
		{
			// �����豸״̬ 
			InvokeEx(() =>
			{
				slightRwer2.LightColor = (status ? Color.Green : Color.Red);

				commonDAO.SetSignalDataValue(CommonAppConfig.GetInstance().AppIdentifier, eSignalDataName.������2_����״̬.ToString(), status ? "1" : "0");
			});
		}

		#endregion

		#region LED��ʾ��

		/// <summary>
		/// ����LED��̬����
		/// </summary>
		/// <param name="value1">��һ������</param>
		/// <param name="value2">�ڶ�������</param>
		private void UpdateLedShow(string value1 = "", string value2 = "")
		{
			UpdateLed1Show(value1, value2);
			UpdateLed2Show(value1, value2);
		}

		#region LED1���ƿ�
		YB_Bx5K1 LED1 = new YB_Bx5K1();
		/// <summary>
		/// LED1���±�ʶ
		/// </summary>
		bool LED1m_bSendBusy = false;

		private bool _LED1ConnectStatus;
		/// <summary>
		/// LED1����״̬
		/// </summary>
		public bool LED1ConnectStatus
		{
			get
			{
				return _LED1ConnectStatus;
			}

			set
			{
				_LED1ConnectStatus = value;

				slightLED1.LightColor = (value ? Color.Green : Color.Red);

				commonDAO.SetSignalDataValue(CommonAppConfig.GetInstance().AppIdentifier, eSignalDataName.LED��1_����״̬.ToString(), value ? "1" : "0");
			}
		}

		/// <summary>
		/// LED1��һ����ʾ����
		/// </summary>
		string LED1PrevLedFileContent = string.Empty;

		/// <summary>
		/// ����LED1��̬����
		/// </summary>
		/// <param name="value1">��һ������</param>
		/// <param name="value2">�ڶ�������</param>
		private void UpdateLed1Show(string value1 = "", string value2 = "")
		{
			if (this.LED1PrevLedFileContent == value1 + value2) return;
			FrmDebugConsole.GetInstance().Output("����LED1:|" + value1 + "|" + value2 + "|");
			if (!this.LED1ConnectStatus) return;

			if (LED1.UpdateArea(value1, value2))
			{
				LED1m_bSendBusy = true;
			}
			else
				LED1m_bSendBusy = false;

			this.LED1PrevLedFileContent = value1 + value2;
		}

		#endregion

		#region LED2���ƿ�
		YB_Bx5K1 LED2 = new YB_Bx5K1();
		/// <summary>
		/// LED2���±�ʶ
		/// </summary>
		bool LED2m_bSendBusy = false;

		private bool _LED2ConnectStatus;
		/// <summary>
		/// LED2����״̬
		/// </summary>
		public bool LED2ConnectStatus
		{
			get
			{
				return _LED2ConnectStatus;
			}

			set
			{
				_LED2ConnectStatus = value;

				slightLED2.LightColor = (value ? Color.Green : Color.Red);

				commonDAO.SetSignalDataValue(CommonAppConfig.GetInstance().AppIdentifier, eSignalDataName.LED��2_����״̬.ToString(), value ? "1" : "0");
			}
		}

		/// <summary>
		/// LED2��һ����ʾ����
		/// </summary>
		string LED2PrevLedFileContent = string.Empty;

		/// <summary>
		/// ����LED2��̬����
		/// </summary>
		/// <param name="value1">��һ������</param>
		/// <param name="value2">�ڶ�������</param>
		private void UpdateLed2Show(string value1 = "", string value2 = "")
		{
			if (this.LED2PrevLedFileContent == value1 + value2) return;
			FrmDebugConsole.GetInstance().Output("����LED2:|" + value1 + "|" + value2 + "|");
			if (!this.LED2ConnectStatus) return;

			if (LED2.UpdateArea(value1, value2))
			{
				LED2m_bSendBusy = true;
			}
			else
				LED2m_bSendBusy = false;


			this.LED2PrevLedFileContent = value1 + value2;
		}

		#endregion

		#endregion

		#region �ذ��Ǳ�

		/// <summary>
		/// �����ȶ��¼�
		/// </summary>
		/// <param name="steady"></param>
		void Wber_OnSteadyChange(bool steady)
		{
			InvokeEx(() =>
			  {
				  this.WbSteady = steady;
			  });
		}

		/// <summary>
		/// �ذ��Ǳ�״̬�仯
		/// </summary>
		/// <param name="status"></param>
		void Wber_OnStatusChange(bool status)
		{
			// �����豸״̬ 
			InvokeEx(() =>
			{
				slightWber.LightColor = (status ? Color.Green : Color.Red);

				commonDAO.SetSignalDataValue(CommonAppConfig.GetInstance().AppIdentifier, eSignalDataName.�ذ��Ǳ�_����״̬.ToString(), status ? "1" : "0");
			});
		}

		void Wber_OnWeightChange(double weight)
		{
			InvokeEx(() =>
			{
				panCurrentWeight.Text = weight.ToString();
			});
		}

		#endregion

		#region ������Ƶ

		/// <summary>
		/// �������������
		/// </summary>
		IPCer iPCer1 = new IPCer();
		IPCer iPCer2 = new IPCer();

		/// <summary>
		/// ִ������ͷץ�ģ�����������
		/// </summary>
		/// <param name="transportId">�����¼Id</param>
		private void CamareCapturePicture(string transportId)
		{
			try
			{
				// ץ����Ƭ������������ַ
				string pictureWebUrl = commonDAO.GetCommonAppletConfigString("�������ܻ�_ץ����Ƭ����·��");

				// �����1
				string picture1FileName = Path.Combine(SelfVars.CapturePicturePath, Guid.NewGuid().ToString() + ".bmp");
				if (iPCer1.CapturePicture(picture1FileName))
				{
					CmcsTransportPicture transportPicture = new CmcsTransportPicture()
					{
						CaptureTime = DateTime.Now,
						CaptureType = CommonAppConfig.GetInstance().AppIdentifier,
						TransportId = transportId,
						PicturePath = pictureWebUrl + Path.GetFileName(picture1FileName)
					};

					if (commonDAO.SelfDber.Insert(transportPicture) > 0) waitForUpload.Enqueue(picture1FileName);
				}

				// �����2
				string picture2FileName = Path.Combine(SelfVars.CapturePicturePath, "Camera", Guid.NewGuid().ToString() + ".bmp");
				if (iPCer2.CapturePicture(picture2FileName))
				{
					CmcsTransportPicture transportPicture = new CmcsTransportPicture()
					{
						CaptureTime = DateTime.Now,
						CaptureType = CommonAppConfig.GetInstance().AppIdentifier,
						TransportId = transportId,
						PicturePath = pictureWebUrl + Path.GetFileName(picture1FileName)
					};

					if (commonDAO.SelfDber.Insert(transportPicture) > 0) waitForUpload.Enqueue(picture2FileName);
				}
			}
			catch (Exception ex)
			{
				Log4Neter.Error("�����ץ��", ex);
			}
		}

		/// <summary>
		/// �ϴ�ץ����Ƭ�������������ļ���
		/// </summary>
		private void UploadCapturePicture()
		{
			string serverPath = commonDAO.GetCommonAppletConfigString("�������ܻ�_ץ����Ƭ����������·��");
			if (string.IsNullOrEmpty(serverPath)) return;

			string fileName = string.Empty;
			while (this.waitForUpload.Count > 0)
			{
				fileName = this.waitForUpload.Dequeue();
				if (!string.IsNullOrEmpty(fileName) && File.Exists(fileName))
				{
					try
					{
						if (Directory.Exists(serverPath)) File.Copy(fileName, Path.Combine(serverPath, Path.GetFileName(fileName)), true);
					}
					catch (Exception ex)
					{
						Log4Neter.Error("�ϴ�ץ����Ƭ", ex);

						break;
					}
				}
			}
		}

		/// <summary>
		/// �������ڵ�ץ����Ƭ
		/// </summary> 
		public void ClearExpireCapturePicture()
		{
			foreach (string item in Directory.GetFiles(SelfVars.CapturePicturePath).Where(a =>
			{
				return new FileInfo(a).LastWriteTime > DateTime.Now.AddMonths(-6);
			}))
			{
				try
				{
					File.Delete(item);
				}
				catch { }
			}
		}

		#endregion

		#region ��������ץ�����

		/// <summary>
		/// �������������
		/// </summary>
		IPCer iPCer_Identify1 = new IPCer();
		IPCer iPCer_Identify2 = new IPCer();

		void ReceiveData1(string carNumber)
		{
			UpdateLed1Show("ʶ�𵽳���:" + carNumber);
			if (this.CurrentFlowFlag == eFlowFlag.�ȴ�����)
			{
				CameraCarNumber = carNumber.Replace("�޳���", "");
				this.CurrentDirection = eDirection.Way1;
				passCarQueuer.Enqueue(eDirection.Way1, CameraCarNumber);
				this.CurrentFlowFlag = eFlowFlag.��֤��Ϣ;
				timer1_Tick(null, null);
			}
		}
		void ReceiveData2(string carNumber)
		{
			UpdateLed2Show("ʶ�𵽳���:" + carNumber);
			if (this.CurrentFlowFlag == eFlowFlag.�ȴ�����)
			{
				CameraCarNumber = carNumber.Replace("�޳���", "");
				this.CurrentDirection = eDirection.Way2;
				passCarQueuer.Enqueue(eDirection.Way2, CameraCarNumber);
				this.CurrentFlowFlag = eFlowFlag.��֤��Ϣ;
				timer1_Tick(null, null);
			}
		}
		#endregion

		#region �豸��ʼ����ж��

		/// <summary>
		/// ��ʼ������豸
		/// </summary>
		private void InitHardware()
		{
			try
			{
				bool success = false;

				this.InductorCoil1Port = commonDAO.GetAppletConfigInt32("IO������_�ظ�1�˿�");
				this.InductorCoil2Port = commonDAO.GetAppletConfigInt32("IO������_�ظ�2�˿�");

				this.InfraredSensor1Port = commonDAO.GetAppletConfigInt32("IO������_����1�˿�");
				this.InfraredSensor2Port = commonDAO.GetAppletConfigInt32("IO������_����2�˿�");
				this.InfraredSensor3Port = commonDAO.GetAppletConfigInt32("IO������_����3�˿�");

				this.WbMinWeight = commonDAO.GetAppletConfigDouble("�ذ��Ǳ�_��С����");
				this.AutoPrint = commonDAO.GetAppletConfigString("�Զ���ӡ����") == "1";

				//��������
				this.IsTicketDiff = commonDAO.GetCommonAppletConfigString("���þ��س�����ֵ") == "1";
				this.TicketDiff = (decimal)commonDAO.GetCommonAppletConfigDouble("���س�����ֵ");

				// IO������
				Hardwarer.Iocer.OnReceived += new IOC.JMDM20DIOV2.JMDM20DIOV2Iocer.ReceivedEventHandler(Iocer_Received);
				Hardwarer.Iocer.OnStatusChange += new IOC.JMDM20DIOV2.JMDM20DIOV2Iocer.StatusChangeHandler(Iocer_StatusChange);
				success = Hardwarer.Iocer.OpenCom(commonDAO.GetAppletConfigInt32("IO������_����"), commonDAO.GetAppletConfigInt32("IO������_������"), commonDAO.GetAppletConfigInt32("IO������_����λ"), (StopBits)commonDAO.GetAppletConfigInt32("IO������_ֹͣλ"), (Parity)commonDAO.GetAppletConfigInt32("IO������_У��λ"));
				if (!success) MessageBoxEx.Show("IO����������ʧ�ܣ�", "������ʾ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				this.iocControler = new IocControler(Hardwarer.Iocer);

				// �ذ��Ǳ�
				Hardwarer.Wber.OnStatusChange += new WB.JinZhong.JinZhongWber.StatusChangeHandler(Wber_OnStatusChange);
				Hardwarer.Wber.OnSteadyChange += new WB.JinZhong.JinZhongWber.SteadyChangeEventHandler(Wber_OnSteadyChange);
				Hardwarer.Wber.OnWeightChange += new WB.JinZhong.JinZhongWber.WeightChangeEventHandler(Wber_OnWeightChange);
				success = Hardwarer.Wber.OpenCom(commonDAO.GetAppletConfigInt32("�ذ��Ǳ�_����"), commonDAO.GetAppletConfigInt32("�ذ��Ǳ�_������"), commonDAO.GetAppletConfigInt32("�ذ��Ǳ�_����λ"), commonDAO.GetAppletConfigInt32("�ذ��Ǳ�_ֹͣλ"), commonDAO.GetAppletConfigInt32("�ذ��Ǳ�_У��λ"));

				IPCer.InitSDK();

				if (this.Direction == "���ϰ�" || this.Direction == "˫���")
				{
					// ������1
					Hardwarer.Rwer1.StartWith = commonDAO.GetAppletConfigString("������_��ǩ����");
					Hardwarer.Rwer1.OnStatusChange += new RW.LZR12.Net.Lzr12Rwer.StatusChangeHandler(Rwer1_OnStatusChange);
					Hardwarer.Rwer1.OnScanError += new RW.LZR12.Net.Lzr12Rwer.ScanErrorEventHandler(Rwer1_OnScanError);
					success = CommonUtil.PingReplyTest(commonDAO.GetAppletConfigString("������1_IP��ַ")) && Hardwarer.Rwer1.OpenCom(commonDAO.GetAppletConfigString("������1_IP��ַ"), 500, Convert.ToByte(commonDAO.GetAppletConfigInt32("������1_����")));
					if (!success) MessageBoxEx.Show("������1����ʧ�ܣ�", "������ʾ", MessageBoxButtons.OK, MessageBoxIcon.Warning);

					#region LED���ƿ�1

					string led1SocketIP = commonDAO.GetAppletConfigString("LED��ʾ��1_IP��ַ");
					if (!string.IsNullOrEmpty(led1SocketIP))
					{
						if (CommonUtil.PingReplyTest(led1SocketIP))
						{
							if (LED1.CreateListent(led1SocketIP))
							{
								// ��ʼ���ɹ�
								this.LED1ConnectStatus = true;
								UpdateLed1Show("  �ȴ�����");
							}
							else
							{
								this.LED1ConnectStatus = false;
								Log4Neter.Error("LED1���ƿ�����ʧ��", new Exception("����ʧ��"));
								MessageBoxEx.Show("LED1���ƿ�����ʧ��", "������ʾ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
							}
						}
						else
						{
							this.LED1ConnectStatus = false;
							Log4Neter.Error("��ʼ��LED1���ƿ�����������ʧ��", new Exception("�����쳣"));
							MessageBoxEx.Show("LED1���ƿ���������ʧ�ܣ�", "������ʾ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
						}
					}

					#endregion

					CmcsCamare video_Identify1 = commonDAO.SelfDber.Entity<CmcsCamare>("where Name=:Name", new { Name = CommonAppConfig.GetInstance().AppIdentifier + "����ʶ��1" });
					if (video_Identify1 != null)
					{
						if (CommonUtil.PingReplyTest(video_Identify1.Ip))
						{
							if (iPCer_Identify1.Login(video_Identify1.Ip, video_Identify1.Port, video_Identify1.UserName, video_Identify1.Password))
							{
								iPCer_Identify1.StartPreview(panVideo1.Handle, video_Identify1.Channel);
								iPCer_Identify1.OnReceived = ReceiveData1;
								iPCer_Identify1.SetDVRCallBack();
								iPCer_Identify1.SetupAlarm();
							}
						}
					}
				}
				if (this.Direction == "���ϰ�" || this.Direction == "˫���")
				{
					// ������2
					Hardwarer.Rwer2.StartWith = commonDAO.GetAppletConfigString("������_��ǩ����");
					Hardwarer.Rwer2.OnStatusChange += new RW.LZR12.Net.Lzr12Rwer.StatusChangeHandler(Rwer2_OnStatusChange);
					Hardwarer.Rwer2.OnScanError += new RW.LZR12.Net.Lzr12Rwer.ScanErrorEventHandler(Rwer2_OnScanError);
					success = CommonUtil.PingReplyTest(commonDAO.GetAppletConfigString("������2_IP��ַ")) && Hardwarer.Rwer2.OpenCom(commonDAO.GetAppletConfigString("������2_IP��ַ"), 500, Convert.ToByte(commonDAO.GetAppletConfigInt32("������2_����")));
					if (!success) MessageBoxEx.Show("������2����ʧ�ܣ�", "������ʾ", MessageBoxButtons.OK, MessageBoxIcon.Warning);

					#region LED���ƿ�2

					string led2SocketIP = commonDAO.GetAppletConfigString("LED��ʾ��2_IP��ַ");
					if (!string.IsNullOrEmpty(led2SocketIP))
					{
						if (CommonUtil.PingReplyTest(led2SocketIP))
						{
							//if (true)
							if (LED2.CreateListent(led2SocketIP))
							{
								// ��ʼ���ɹ�
								this.LED2ConnectStatus = true;
								UpdateLed2Show("  �ȴ�����");
							}
							else
							{
								this.LED2ConnectStatus = false;
								Log4Neter.Error("LED2���ƿ�����ʧ��", new Exception("����ʧ��"));
								MessageBoxEx.Show("LED2���ƿ�����ʧ��", "������ʾ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
							}
						}
						else
						{
							this.LED2ConnectStatus = false;
							Log4Neter.Error("��ʼ��LED2���ƿ�����������ʧ��", new Exception("�����쳣"));
							MessageBoxEx.Show("LED2���ƿ���������ʧ�ܣ�", "������ʾ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
						}
					}

					#endregion

					CmcsCamare video_Identify2 = commonDAO.SelfDber.Entity<CmcsCamare>("where Name=:Name", new { Name = CommonAppConfig.GetInstance().AppIdentifier + "����ʶ��2" });
					if (video_Identify2 != null)
					{
						if (CommonUtil.PingReplyTest(video_Identify2.Ip))
						{
							if (iPCer_Identify2.Login(video_Identify2.Ip, video_Identify2.Port, video_Identify2.UserName, video_Identify2.Password))
							{
								iPCer_Identify2.StartPreview(panVideo2.Handle, video_Identify2.Channel);
								iPCer_Identify2.OnReceived = ReceiveData2;
								iPCer_Identify2.SetDVRCallBack();
								iPCer_Identify2.SetupAlarm();
							}
						}
					}
				}

				#region ������Ƶ

				CmcsCamare video1 = commonDAO.SelfDber.Entity<CmcsCamare>("where Name=:Name", new { Name = CommonAppConfig.GetInstance().AppIdentifier + "��Ƶ���1" });
				if (video1 != null)
				{
					if (CommonUtil.PingReplyTest(video1.Ip))
					{
						if (iPCer1.Login(video1.Ip, video1.Port, video1.UserName, video1.Password))
						{
							bool res = iPCer1.StartPreview(panVideo1.Handle, video1.Channel);
						}
					}
				}

				CmcsCamare video2 = commonDAO.SelfDber.Entity<CmcsCamare>("where Name=:Name", new { Name = CommonAppConfig.GetInstance().AppIdentifier + "��Ƶ���2" });
				if (video2 != null)
				{
					if (CommonUtil.PingReplyTest(video2.Ip))
					{
						if (iPCer2.Login(video2.Ip, video2.Port, video2.UserName, video2.Password))
							iPCer2.StartPreview(panVideo2.Handle, video2.Channel);
					}
				}

				#endregion

				//��������
				voiceSpeaker.SetVoice(commonDAO.GetAppletConfigInt32("����"), commonDAO.GetAppletConfigInt32("����"), commonDAO.GetAppletConfigString("������"));

				timer1.Enabled = true;
			}
			catch (Exception ex)
			{
				Log4Neter.Error("�豸��ʼ��", ex);
			}
		}

		/// <summary>
		/// ж���豸
		/// </summary>
		private void UnloadHardware()
		{
			// ע��˶δ���
			Application.DoEvents();

			try
			{
				commonDAO.SetSignalDataValue(CommonAppConfig.GetInstance().AppIdentifier, eSignalDataName.��ǰ��Id.ToString(), string.Empty);
				commonDAO.SetSignalDataValue(CommonAppConfig.GetInstance().AppIdentifier, eSignalDataName.��ǰ����.ToString(), string.Empty);
			}
			catch { }
			try
			{
				Hardwarer.Iocer.OnReceived -= new IOC.JMDM20DIOV2.JMDM20DIOV2Iocer.ReceivedEventHandler(Iocer_Received);
				Hardwarer.Iocer.OnStatusChange -= new IOC.JMDM20DIOV2.JMDM20DIOV2Iocer.StatusChangeHandler(Iocer_StatusChange);

				Hardwarer.Iocer.CloseCom();
			}
			catch { }
			try
			{
				Hardwarer.Rwer1.CloseCom();
			}
			catch { }
			try
			{
				Hardwarer.Rwer2.CloseCom();
			}
			catch { }
			try
			{
				if (this.LED1ConnectStatus)
				{
					LED1.CloseListent();
				}
			}
			catch { }
			try
			{
				if (this.LED2ConnectStatus)
				{
					LED2.CloseListent();
				}
			}
			catch { }
			try
			{
				iPCer_Identify1.OnReceived = null;
				iPCer_Identify1.CloseAlarm();
				iPCer_Identify1.LoginOut();

				iPCer_Identify2.OnReceived = null;
				iPCer_Identify2.CloseAlarm();
				iPCer_Identify2.LoginOut();

				IPCer.CleanupSDK();
			}
			catch { }
		}

		#endregion

		#endregion

		#region ��բ���ư�ť

		/// <summary>
		/// ��բ1����
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void btnGate1Up_Click(object sender, EventArgs e)
		{
			if (this.iocControler != null) this.iocControler.Gate1Up();
		}

		/// <summary>
		/// ��բ1����
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void btnGate1Down_Click(object sender, EventArgs e)
		{
			if (this.iocControler != null) this.iocControler.Gate1Down();
		}

		/// <summary>
		/// ��բ2����
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void btnGate2Up_Click(object sender, EventArgs e)
		{
			if (this.iocControler != null) this.iocControler.Gate2Up();
		}

		/// <summary>
		/// ��բ2����
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void btnGate2Down_Click(object sender, EventArgs e)
		{
			if (this.iocControler != null) this.iocControler.Gate2Down();
		}

		#endregion

		#region ����ҵ��

		/// <summary>
		/// ����������ʶ������
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void timer1_Tick(object sender, EventArgs e)
		{
			timer1.Stop();
			timer1.Interval = 2000;

			try
			{
				// ִ��Զ������
				//ExecAppRemoteControlCmd();

				switch (this.CurrentFlowFlag)
				{
					case eFlowFlag.�ȴ�����:
						#region

						// Direction.Way1
						if ((this.InductorCoil1 || this.InfraredSensor1))
						{
							// ����������ظ����źţ������������߳���ʶ��
							this.CurrentDirection = eDirection.Way1;

							this.CurrentFlowFlag = eFlowFlag.��ʼ����;
						}
						// Direction.Way2
						else if (this.InductorCoil2 || this.InfraredSensor3)
						{
							// ����������ظ����źţ������������߳���ʶ��
							this.CurrentDirection = eDirection.Way2;

							this.CurrentFlowFlag = eFlowFlag.��ʼ����;
						}

						if (passCarQueuer.Count > 0)
						{
							this.CurrentFlowFlag = eFlowFlag.��ʼ����;
							this.CurrentDirection = passCarQueuer.Dequeue().PassWay;
						}
						#endregion
						break;

					case eFlowFlag.��ʼ����:
						#region

						//��߶���������
						timer1.Interval = 500;

						if (this.CurrentDirection == eDirection.Way1)
						{
							List<string> tags = Hardwarer.Rwer1.ScanTags();
							if (tags.Count > 0) passCarQueuer.Enqueue(eDirection.Way1, tags[0]);

							commonDAO.SetSignalDataValue(CommonAppConfig.GetInstance().AppIdentifier, eSignalDataName.�ϰ�����.ToString(), "0");

						}
						else if (this.CurrentDirection == eDirection.Way2)
						{
							List<string> tags = Hardwarer.Rwer2.ScanTags();
							if (tags.Count > 0) passCarQueuer.Enqueue(eDirection.Way2, tags[0]);

							commonDAO.SetSignalDataValue(CommonAppConfig.GetInstance().AppIdentifier, eSignalDataName.�ϰ�����.ToString(), "1");
						}

						if (passCarQueuer.Count > 0) this.CurrentFlowFlag = eFlowFlag.ʶ����;


						UpdateLedShow("  ���ڶ���");

						#endregion
						break;

					case eFlowFlag.ʶ����:
						#region

						// �������޳�ʱ���ȴ�����
						if (passCarQueuer.Count == 0)
						{
							UpdateLedShow("  �ȴ�����");
							this.CurrentImperfectCar = null;
							this.CurrentDirection = eDirection.UnKnow;
							this.CurrentFlowFlag = eFlowFlag.�ȴ�����;
							break;
						}

						this.CurrentImperfectCar = passCarQueuer.Dequeue();

						// ��ʽһ������ʶ��ĳ��ƺŲ��ҳ�����Ϣ
						this.CurrentAutotruck = carTransportDAO.GetAutotruckByCarNumber(this.CameraCarNumber);
						if (this.CurrentAutotruck == null)
							this.CurrentAutotruck = carTransportDAO.GetAutotruckByCarNumber(this.CurrentImperfectCar.Voucher);
						if (this.CurrentAutotruck == null)
							// ��ʽ��������ʶ��ı�ǩ�����ҳ�����Ϣ
							this.CurrentAutotruck = carTransportDAO.GetAutotruckByTagId(this.CurrentImperfectCar.Voucher);

						if (this.CurrentAutotruck != null)
						{
							UpdateLedShow(this.CurrentAutotruck.CarNumber + "ʶ��ɹ�");
							this.voiceSpeaker.Speak(this.CurrentAutotruck.CarNumber + " ʶ��ɹ�", 1, false);

							if (this.CurrentAutotruck.IsUse == 1)
							{
								if (this.CurrentAutotruck.CarType == eCarType.�볧ú.ToString())
								{
									this.timer_BuyFuel_Cancel = false;
									this.CurrentFlowFlag = eFlowFlag.��֤��Ϣ;
								}
								else if (this.CurrentAutotruck.CarType == eCarType.��������.ToString())
								{
									this.timer_Goods_Cancel = false;
									this.CurrentFlowFlag = eFlowFlag.��֤��Ϣ;
								}
							}
							else
							{
								UpdateLedShow(this.CurrentAutotruck.CarNumber, "��ͣ��");
								this.voiceSpeaker.Speak("���ƺ� " + this.CurrentAutotruck.CarNumber + " ��ͣ�ã���ֹͨ��", 1, false);

								timer1.Interval = 20000;
							}
						}
						else
						{
							UpdateLedShow(this.CurrentImperfectCar.Voucher, "δ�Ǽ�");

							//��ʽһ������ʶ��
							this.voiceSpeaker.Speak("���ƺ� " + this.CurrentImperfectCar.Voucher + " δ�Ǽǣ���ֹͨ��", 1, false);
							//// ��ʽ����ˢ����ʽ
							//this.voiceSpeaker.Speak("����δ�Ǽǣ���ֹͨ��", 2, false);

							timer1.Interval = 20000;
						}

						#endregion
						break;
				}

				// ��ǰ�ذ�����С����С���������еظС��������ź�ʱ����
				if (Hardwarer.Wber.Weight < this.WbMinWeight && !HasCarOnEnterWay() && !HasCarOnLeaveWay() && this.CurrentFlowFlag != eFlowFlag.�ȴ����� && this.CurrentImperfectCar != null && string.IsNullOrEmpty(this.CameraCarNumber))
				{
					ResetBuyFuel();
					ResetGoods();
				}
				commonDAO.SetSignalDataValue(CommonAppConfig.GetInstance().AppIdentifier, eSignalDataName.�ذ��Ǳ�_ʵʱ����.ToString(), Hardwarer.Wber.Weight.ToString());
			}
			catch (Exception ex)
			{
				Log4Neter.Error("timer1_Tick", ex);
			}
			finally
			{
				timer1.Start();
			}
		}

		/// <summary>
		/// ��������
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void timer2_Tick(object sender, EventArgs e)
		{
			timer2.Stop();
			// ������ִ��һ��
			timer2.Interval = 180000;

			try
			{
				// �ϴ�ץ����Ƭ
				UploadCapturePicture();
				// ����ץ����Ƭ
				if (DateTime.Now.Hour == 0) ClearExpireCapturePicture();
			}
			catch (Exception ex)
			{
				Log4Neter.Error("timer2_Tick", ex);
			}
			finally
			{
				timer2.Start();
			}
		}

		/// <summary>
		/// �г������ϰ��ĵ�·��
		/// </summary>
		/// <returns></returns>
		bool HasCarOnEnterWay()
		{
			if (this.CurrentImperfectCar == null) return false;

			if (this.CurrentImperfectCar.PassWay == eDirection.UnKnow)
				return false;
			else if (this.CurrentImperfectCar.PassWay == eDirection.Way1)
				return this.InductorCoil1 || this.InfraredSensor1;
			else if (this.CurrentImperfectCar.PassWay == eDirection.Way2)
				return this.InductorCoil2 || this.InfraredSensor3;

			return true;
		}

		/// <summary>
		/// �г������°��ĵ�·��
		/// </summary>
		/// <returns></returns>
		bool HasCarOnLeaveWay()
		{
			if (this.CurrentImperfectCar == null) return false;

			if (this.CurrentImperfectCar.PassWay == eDirection.UnKnow)
				return false;
			else if (this.CurrentImperfectCar.PassWay == eDirection.Way1)
				return this.InductorCoil2 || this.InfraredSensor3;
			else if (this.CurrentImperfectCar.PassWay == eDirection.Way2)
				return this.InductorCoil1 || this.InfraredSensor1;

			return true;
		}

		/// <summary>
		/// ִ��Զ������
		/// </summary>
		void ExecAppRemoteControlCmd()
		{
			// ��ȡ���µ�����
			CmcsAppRemoteControlCmd appRemoteControlCmd = commonDAO.GetNewestAppRemoteControlCmd(CommonAppConfig.GetInstance().AppIdentifier);
			if (appRemoteControlCmd != null)
			{
				if (appRemoteControlCmd.CmdCode == "���Ƶ�բ")
				{
					Log4Neter.Info("����Զ�����" + appRemoteControlCmd.CmdCode + "��������" + appRemoteControlCmd.Param);

					if (appRemoteControlCmd.Param.Equals("Gate1Up", StringComparison.CurrentCultureIgnoreCase))
						this.iocControler.Gate1Up();
					else if (appRemoteControlCmd.Param.Equals("Gate1Down", StringComparison.CurrentCultureIgnoreCase))
						this.iocControler.Gate1Down();
					else if (appRemoteControlCmd.Param.Equals("Gate2Up", StringComparison.CurrentCultureIgnoreCase))
						this.iocControler.Gate2Up();
					else if (appRemoteControlCmd.Param.Equals("Gate2Down", StringComparison.CurrentCultureIgnoreCase))
						this.iocControler.Gate2Down();

					// ����ִ�н��
					commonDAO.SetAppRemoteControlCmdResultCode(appRemoteControlCmd, eEquInfCmdResultCode.�ɹ�);
				}
			}
		}

		/// <summary>
		/// �л��ֶ�/�Զ�ģʽ
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void sbtnChangeAutoHandMode_ValueChanged(object sender, EventArgs e)
		{
			this.AutoHandMode = sbtnChangeAutoHandMode.Value;
		}

		/// <summary>
		/// ˢ���б�
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void btnRefresh_Click(object sender, EventArgs e)
		{
			// �볧ú
			LoadTodayUnFinishBuyFuelTransport();
			LoadTodayFinishBuyFuelTransport();

			// ��������
			LoadTodayUnFinishGoodsTransport();
			LoadTodayFinishGoodsTransport();
		}

		#endregion

		#region �볧úҵ��

		bool timer_BuyFuel_Cancel = true;

		CmcsBuyFuelTransport currentBuyFuelTransport;
		/// <summary>
		/// ��ǰ�����¼
		/// </summary>
		public CmcsBuyFuelTransport CurrentBuyFuelTransport
		{
			get { return currentBuyFuelTransport; }
			set
			{
				currentBuyFuelTransport = value;

				if (value != null)
				{
					commonDAO.SetSignalDataValue(CommonAppConfig.GetInstance().AppIdentifier, eSignalDataName.��ǰ�����¼Id.ToString(), value.Id);

					txtFuelKindName_BuyFuel.Text = value.FuelKindName;
					txtMineName_BuyFuel.Text = value.MineName;
					txtSupplierName_BuyFuel.Text = value.SupplierName;
					txtTransportCompanyName_BuyFuel.Text = value.TransportCompanyName;

					txtGrossWeight_BuyFuel.Text = value.GrossWeight.ToString("F2");
					txtTicketWeight_BuyFuel.Text = value.TicketWeight.ToString("F2");
					txtTareWeight_BuyFuel.Text = value.TareWeight.ToString("F2");
					txtDeductWeight_BuyFuel.Text = value.DeductWeight.ToString("F2");
					txtSuttleWeight_BuyFuel.Text = value.SuttleWeight.ToString("F2");
				}
				else
				{
					commonDAO.SetSignalDataValue(CommonAppConfig.GetInstance().AppIdentifier, eSignalDataName.��ǰ�����¼Id.ToString(), string.Empty);

					txtFuelKindName_BuyFuel.ResetText();
					txtMineName_BuyFuel.ResetText();
					txtSupplierName_BuyFuel.ResetText();
					txtTransportCompanyName_BuyFuel.ResetText();

					txtGrossWeight_BuyFuel.ResetText();
					txtTicketWeight_BuyFuel.ResetText();
					txtTareWeight_BuyFuel.ResetText();
					txtDeductWeight_BuyFuel.ResetText();
					txtSuttleWeight_BuyFuel.ResetText();
				}
			}
		}

		/// <summary>
		/// ѡ����
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void btnSelectAutotruck_BuyFuel_Click(object sender, EventArgs e)
		{
			FrmUnFinishTransport_Select frm = new FrmUnFinishTransport_Select("where CarType='" + eCarType.�볧ú.ToString() + "' order by CreationTime desc");
			if (frm.ShowDialog() == DialogResult.OK)
			{
				if (this.InductorCoil1)
					passCarQueuer.Enqueue(eDirection.Way1, frm.Output.CarNumber);
				else if (this.InductorCoil2)
					passCarQueuer.Enqueue(eDirection.Way2, frm.Output.CarNumber);
				else
					passCarQueuer.Enqueue(eDirection.UnKnow, frm.Output.CarNumber);

				this.CurrentFlowFlag = eFlowFlag.ʶ����;
			}
		}

		/// <summary>
		/// �����볧ú�����¼
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void btnSaveTransport_BuyFuel_Click(object sender, EventArgs e)
		{
			if (!SaveBuyFuelTransport()) MessageBoxEx.Show("����ʧ��", "������ʾ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
		}

		/// <summary>
		/// ���������¼
		/// </summary>
		/// <returns></returns>
		bool SaveBuyFuelTransport()
		{
			if (this.CurrentBuyFuelTransport == null) return false;

			try
			{
				bool isDiff = false;
				decimal diffValue = 0;

				decimal Weight = (decimal)Hardwarer.Wber.Weight;

				if (this.CurrentBuyFuelTransport.StepName == eTruckInFactoryStep.�س�.ToString())
				{
					//���þ��س���
					if (this.IsTicketDiff && this.CurrentBuyFuelTransport.TicketWeight > 0)
					{
						isDiff = Math.Abs(this.CurrentBuyFuelTransport.GrossWeight - Weight - this.CurrentBuyFuelTransport.TicketWeight) - this.TicketDiff > 0;
						diffValue = Math.Abs(this.CurrentBuyFuelTransport.GrossWeight - Weight - this.CurrentBuyFuelTransport.TicketWeight);

						if (isDiff)
						{
							UpdateLedShow(this.CurrentAutotruck.CarNumber + "���س��" + diffValue + "��");
							this.voiceSpeaker.Speak(this.CurrentAutotruck.CarNumber + "���س��" + diffValue + "��", 1, false);
							return false;
						}
					}
				}

				if (weighterDAO.SaveBuyFuelTransport(this.CurrentBuyFuelTransport.Id, Weight, DateTime.Now, CommonAppConfig.GetInstance().AppIdentifier))
				{
					this.CurrentBuyFuelTransport = commonDAO.SelfDber.Get<CmcsBuyFuelTransport>(this.CurrentBuyFuelTransport.Id);

					FrontGateUp();

					btnSaveTransport_BuyFuel.Enabled = false;
					this.CurrentFlowFlag = eFlowFlag.�ȴ��뿪;

					UpdateLedShow("�������", "���°�");
					this.voiceSpeaker.Speak("����������°�", 1, false);

					LoadTodayUnFinishBuyFuelTransport();
					LoadTodayFinishBuyFuelTransport();

					CamareCapturePicture(this.CurrentBuyFuelTransport.Id);

					//��ӡ���� 
					if (this.AutoPrint && this.CurrentBuyFuelTransport.SuttleWeight > 0)
					{
						//�첽��ӡ
						new Task(() => { PrintWeightReport.GetInstance(new PrintDocument()).PrintBuyFuelTransport(this.CurrentBuyFuelTransport); }).Start();
					}

					return true;
				}
			}
			catch (Exception ex)
			{
				UpdateLedShow("����ʧ��" + ex.Message);

				Log4Neter.Error("���������¼", ex);
			}

			return false;
		}

		/// <summary>
		/// �����볧ú�����¼
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void btnReset_BuyFuel_Click(object sender, EventArgs e)
		{
			ResetBuyFuel();
		}

		/// <summary>
		/// ������Ϣ
		/// </summary>
		void ResetBuyFuel()
		{
			this.timer_BuyFuel_Cancel = true;

			this.CurrentFlowFlag = eFlowFlag.�ȴ�����;

			this.CurrentAutotruck = null;
			this.CurrentBuyFuelTransport = null;
			this.CurrentDirection = eDirection.UnKnow;

			txtTagId_BuyFuel.ResetText();

			btnSaveTransport_BuyFuel.Enabled = false;
			this.CameraCarNumber = string.Empty;

			FrontGateDown();
			BackGateDown();

			UpdateLedShow("  �ȴ�����");

			// �������
			this.CurrentImperfectCar = null;
		}

		/// <summary>
		/// �볧ú�����¼ҵ��ʱ��
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void timer_BuyFuel_Tick(object sender, EventArgs e)
		{
			if (this.timer_BuyFuel_Cancel) return;

			timer_BuyFuel.Stop();
			timer_BuyFuel.Interval = 2000;

			try
			{
				switch (this.CurrentFlowFlag)
				{
					case eFlowFlag.��֤��Ϣ:
						#region

						// ���Ҹó�δ��ɵ������¼
						CmcsUnFinishTransport unFinishTransport = carTransportDAO.GetUnFinishTransportByAutotruckId(this.CurrentAutotruck.Id, eCarType.�볧ú.ToString());
						if (unFinishTransport != null)
						{
							this.CurrentBuyFuelTransport = commonDAO.SelfDber.Get<CmcsBuyFuelTransport>(unFinishTransport.TransportId);
							if (this.CurrentBuyFuelTransport != null)
							{
								// �ж�·������
								string nextPlace;
								if (carTransportDAO.CheckNextTruckInFactoryWay(this.CurrentAutotruck.CarType, this.CurrentBuyFuelTransport.StepName, "�س�|�ᳵ", CommonAppConfig.GetInstance().AppIdentifier, out nextPlace))
								{
									if (this.CurrentBuyFuelTransport.SuttleWeight == 0)
									{
										BackGateUp();

										this.CurrentFlowFlag = eFlowFlag.�ȴ��ϰ�;

										UpdateLedShow(this.CurrentAutotruck.CarNumber, "���ϰ�");
										this.voiceSpeaker.Speak("���ƺ� " + this.CurrentAutotruck.CarNumber + " ���ϰ�", 1, false);
									}
									else
									{
										UpdateLedShow(this.CurrentAutotruck.CarNumber, "�ѳ���");
										this.voiceSpeaker.Speak("���ƺ� " + this.CurrentAutotruck.CarNumber + " �ѳ���", 1, false);

										timer_BuyFuel.Interval = 20000;
									}
								}
								else
								{
									UpdateLedShow("·�ߴ���", "��ֹͨ��");
									this.voiceSpeaker.Speak("·�ߴ��� ��ֹͨ�� " + (!string.IsNullOrEmpty(nextPlace) ? "��ǰ��" + nextPlace : ""), 1, false);

									timer_BuyFuel.Interval = 20000;
								}
							}
							else
							{
								commonDAO.SelfDber.Delete<CmcsUnFinishTransport>(unFinishTransport.Id);
							}
						}
						else
						{
							UpdateLedShow(this.CurrentAutotruck.CarNumber, "δ�Ŷ�");
							this.voiceSpeaker.Speak("���ƺ� " + this.CurrentAutotruck.CarNumber + " δ�ҵ��ŶӼ�¼", 1, false);

							timer_BuyFuel.Interval = 20000;
						}

						#endregion
						break;

					case eFlowFlag.�ȴ��ϰ�:
						#region

						// ���ذ��Ǳ�����������С��������������ĵظ����������źţ����ж����Ѿ���ȫ�ϰ�
						if (Hardwarer.Wber.Weight >= this.WbMinWeight && !HasCarOnEnterWay())
						{
							BackGateDown();

							this.CurrentFlowFlag = eFlowFlag.�ȴ��ȶ�;
						}

						// ����������
						timer_BuyFuel.Interval = 4000;

						#endregion
						break;

					case eFlowFlag.�ȴ��ȶ�:
						#region

						// ���������
						timer_BuyFuel.Interval = 1000;

						btnSaveTransport_BuyFuel.Enabled = this.WbSteady;

						UpdateLedShow(this.CurrentAutotruck.CarNumber, Hardwarer.Wber.Weight.ToString("#0.######"));

						if (this.WbSteady)
						{
							if (this.AutoHandMode)
							{
								// �Զ�ģʽ
								if (!SaveBuyFuelTransport())
								{
									// ����������
									timer_BuyFuel.Interval = 8000;

									UpdateLedShow(this.CurrentAutotruck.CarNumber, "����ʧ��");
									this.voiceSpeaker.Speak("���ƺ� " + this.CurrentAutotruck.CarNumber + " ����ʧ�ܣ�����ϵ����Ա", 1, false);
								}
							}
							else
							{
								// �ֶ�ģʽ 
							}
						}

						#endregion
						break;

					case eFlowFlag.�ȴ��뿪:
						#region

						// ��ǰ�ذ�����С����С���������еظС��������ź�ʱ����
						if (Hardwarer.Wber.Weight < this.WbMinWeight && !HasCarOnLeaveWay()) ResetBuyFuel();

						// ����������
						timer_BuyFuel.Interval = 4000;

						#endregion
						break;
				}

				// ��ǰ�ذ�����С����С���������еظС��������ź�ʱ����
				if (Hardwarer.Wber.Weight < this.WbMinWeight && !HasCarOnEnterWay() && !HasCarOnLeaveWay() && this.CurrentFlowFlag != eFlowFlag.�ȴ�����
					&& this.CurrentImperfectCar != null) ResetBuyFuel();
			}
			catch (Exception ex)
			{
				Log4Neter.Error("timer_BuyFuel_Tick", ex);
			}
			finally
			{
				timer_BuyFuel.Start();
			}
		}

		/// <summary>
		/// ��ȡδ��ɵ��볧ú��¼
		/// </summary>
		void LoadTodayUnFinishBuyFuelTransport()
		{
			superGridControl1_BuyFuel.PrimaryGrid.DataSource = weighterDAO.GetUnFinishBuyFuelTransport();
		}

		/// <summary>
		/// ��ȡָ����������ɵ��볧ú��¼
		/// </summary>
		void LoadTodayFinishBuyFuelTransport()
		{
			superGridControl2_BuyFuel.PrimaryGrid.DataSource = weighterDAO.GetFinishedBuyFuelTransport(DateTime.Now.Date, DateTime.Now.Date.AddDays(1));
		}

		#endregion

		#region ��������ҵ��

		bool timer_Goods_Cancel = true;

		CmcsGoodsTransport currentGoodsTransport;
		/// <summary>
		/// ��ǰ�����¼
		/// </summary>
		public CmcsGoodsTransport CurrentGoodsTransport
		{
			get { return currentGoodsTransport; }
			set
			{
				currentGoodsTransport = value;

				if (value != null)
				{
					commonDAO.SetSignalDataValue(CommonAppConfig.GetInstance().AppIdentifier, eSignalDataName.��ǰ�����¼Id.ToString(), value.Id);

					txtSupplyUnitName_Goods.Text = value.SupplyUnitName;
					txtReceiveUnitName_Goods.Text = value.ReceiveUnitName;
					txtGoodsTypeName_Goods.Text = value.GoodsTypeName;

					txtFirstWeight_Goods.Text = value.FirstWeight.ToString("F2");
					txtSecondWeight_Goods.Text = value.SecondWeight.ToString("F2");
					txtSuttleWeight_Goods.Text = value.SuttleWeight.ToString("F2");
				}
				else
				{
					commonDAO.SetSignalDataValue(CommonAppConfig.GetInstance().AppIdentifier, eSignalDataName.��ǰ�����¼Id.ToString(), string.Empty);

					txtSupplyUnitName_Goods.ResetText();
					txtReceiveUnitName_Goods.ResetText();
					txtGoodsTypeName_Goods.ResetText();

					txtFirstWeight_Goods.ResetText();
					txtSecondWeight_Goods.ResetText();
					txtSuttleWeight_Goods.ResetText();
				}
			}
		}

		/// <summary>
		/// ѡ����
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void btnSelectAutotruck_Goods_Click(object sender, EventArgs e)
		{
			FrmUnFinishTransport_Select frm = new FrmUnFinishTransport_Select("where CarType='" + eCarType.��������.ToString() + "' order by CreationTime desc");
			if (frm.ShowDialog() == DialogResult.OK)
			{
				if (this.InductorCoil1)
					passCarQueuer.Enqueue(eDirection.Way1, frm.Output.CarNumber);
				else if (this.InductorCoil2)
					passCarQueuer.Enqueue(eDirection.Way2, frm.Output.CarNumber);
				else
					passCarQueuer.Enqueue(eDirection.UnKnow, frm.Output.CarNumber);

				this.CurrentFlowFlag = eFlowFlag.ʶ����;
			}
		}

		/// <summary>
		/// �����ŶӼ�¼
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void btnSaveTransport_Goods_Click(object sender, EventArgs e)
		{
			if (!SaveGoodsTransport()) MessageBoxEx.Show("����ʧ��", "������ʾ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
		}

		/// <summary>
		/// ���������¼
		/// </summary>
		/// <returns></returns>
		bool SaveGoodsTransport()
		{
			if (this.CurrentGoodsTransport == null) return false;

			try
			{
				if (weighterDAO.SaveGoodsTransport(this.CurrentGoodsTransport.Id, (decimal)Hardwarer.Wber.Weight, DateTime.Now, CommonAppConfig.GetInstance().AppIdentifier))
				{
					this.CurrentGoodsTransport = commonDAO.SelfDber.Get<CmcsGoodsTransport>(this.CurrentGoodsTransport.Id);

					FrontGateUp();

					btnSaveTransport_Goods.Enabled = false;
					this.CurrentFlowFlag = eFlowFlag.�ȴ��뿪;

					UpdateLedShow("�������", "���°�");
					this.voiceSpeaker.Speak("����������°�", 1, false);

					LoadTodayUnFinishGoodsTransport();
					LoadTodayFinishGoodsTransport();

					CamareCapturePicture(this.CurrentGoodsTransport.Id);

					//��ӡ���� 
					if (this.AutoPrint && this.CurrentGoodsTransport.SuttleWeight > 0)
					{
						//�첽��ӡ
						new Task(() => { PrintWeightReport.GetInstance(new PrintDocument()).PrintGoodsTransport(this.CurrentGoodsTransport); }).Start();
					}

					return true;
				}
			}
			catch (Exception ex)
			{
				MessageBoxEx.Show("����ʧ��\r\n" + ex.Message, "������ʾ", MessageBoxButtons.OK, MessageBoxIcon.Error);

				Log4Neter.Error("���������¼", ex);
			}

			return false;
		}

		/// <summary>
		/// ������Ϣ
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void btnReset_Goods_Click(object sender, EventArgs e)
		{
			ResetGoods();
		}

		/// <summary>
		/// ������Ϣ
		/// </summary>
		void ResetGoods()
		{
			this.timer_Goods_Cancel = true;

			this.CurrentFlowFlag = eFlowFlag.�ȴ�����;

			this.CurrentAutotruck = null;
			this.CurrentGoodsTransport = null;
			this.CurrentDirection = eDirection.UnKnow;

			txtTagId_Goods.ResetText();

			btnSaveTransport_Goods.Enabled = false;
			this.CameraCarNumber = string.Empty;

			FrontGateDown();
			BackGateDown();

			UpdateLedShow("  �ȴ�����");

			// �������
			this.CurrentImperfectCar = null;
		}

		/// <summary>
		/// �������������¼ҵ��ʱ��
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void timer_Goods_Tick(object sender, EventArgs e)
		{
			if (this.timer_Goods_Cancel) return;

			timer_Goods.Stop();
			timer_Goods.Interval = 2000;

			try
			{
				switch (this.CurrentFlowFlag)
				{
					case eFlowFlag.��֤��Ϣ:
						#region

						// ���Ҹó�δ��ɵ������¼
						CmcsUnFinishTransport unFinishTransport = carTransportDAO.GetUnFinishTransportByAutotruckId(this.CurrentAutotruck.Id, eCarType.��������.ToString());
						if (unFinishTransport != null)
						{
							this.CurrentGoodsTransport = commonDAO.SelfDber.Get<CmcsGoodsTransport>(unFinishTransport.TransportId);
							if (this.CurrentGoodsTransport != null)
							{
								// �ж�·������
								string nextPlace;
								if (carTransportDAO.CheckNextTruckInFactoryWay(this.CurrentAutotruck.CarType, this.CurrentGoodsTransport.StepName, "��һ�γ���|�ڶ��γ���", CommonAppConfig.GetInstance().AppIdentifier, out nextPlace))
								{
									if (this.CurrentGoodsTransport.SuttleWeight == 0)
									{
										BackGateUp();

										this.CurrentFlowFlag = eFlowFlag.�ȴ��ϰ�;

										UpdateLedShow(this.CurrentAutotruck.CarNumber, "���ϰ�");
										this.voiceSpeaker.Speak("���ƺ� " + this.CurrentAutotruck.CarNumber + " ���ϰ�", 1, false);
									}
									else
									{
										UpdateLedShow(this.CurrentAutotruck.CarNumber, "�ѳ���");
										this.voiceSpeaker.Speak("���ƺ� " + this.CurrentAutotruck.CarNumber + " �ѳ���", 1, false);

										timer_Goods.Interval = 20000;
									}
								}
								else
								{
									UpdateLedShow("·�ߴ���", "��ֹͨ��");
									this.voiceSpeaker.Speak("·�ߴ��� ��ֹͨ�� " + (!string.IsNullOrEmpty(nextPlace) ? "��ǰ��" + nextPlace : ""), 1, false);

									timer_Goods.Interval = 20000;
								}
							}
							else
							{
								commonDAO.SelfDber.Delete<CmcsUnFinishTransport>(unFinishTransport.Id);
							}
						}
						else
						{
							UpdateLedShow(this.CurrentAutotruck.CarNumber, "δ�Ŷ�");
							this.voiceSpeaker.Speak("���ƺ� " + this.CurrentAutotruck.CarNumber + " δ�ҵ��ŶӼ�¼", 1, false);

							timer_Goods.Interval = 20000;
						}

						#endregion
						break;

					case eFlowFlag.�ȴ��ϰ�:
						#region

						// ���ذ��Ǳ�����������С��������������ĵظ����������źţ����ж����Ѿ���ȫ�ϰ�
						if (Hardwarer.Wber.Weight >= this.WbMinWeight && !HasCarOnEnterWay())
						{
							BackGateDown();

							this.CurrentFlowFlag = eFlowFlag.�ȴ��ȶ�;
						}

						// ����������
						timer_Goods.Interval = 4000;

						#endregion
						break;

					case eFlowFlag.�ȴ��ȶ�:
						#region

						// ���������
						timer_Goods.Interval = 1000;

						btnSaveTransport_Goods.Enabled = this.WbSteady;

						UpdateLedShow(this.CurrentAutotruck.CarNumber, Hardwarer.Wber.Weight.ToString("#0.######"));

						if (this.WbSteady)
						{
							if (this.AutoHandMode)
							{
								// �Զ�ģʽ
								if (!SaveGoodsTransport())
								{
									UpdateLedShow(this.CurrentAutotruck.CarNumber, "����ʧ��");
									this.voiceSpeaker.Speak("���ƺ� " + this.CurrentAutotruck.CarNumber + " ����ʧ�ܣ�����ϵ����Ա", 1, false);
								}
							}
							else
							{
								// �ֶ�ģʽ 
							}
						}

						#endregion
						break;

					case eFlowFlag.�ȴ��뿪:
						#region

						// ��ǰ�ذ�����С����С���������еظС��������ź�ʱ����
						if (Hardwarer.Wber.Weight < this.WbMinWeight && !HasCarOnLeaveWay()) ResetGoods();

						// ����������
						timer_Goods.Interval = 4000;

						#endregion
						break;
				}

				// ��ǰ�ذ�����С����С���������еظС��������ź�ʱ����
				if (Hardwarer.Wber.Weight < this.WbMinWeight && !HasCarOnEnterWay() && !HasCarOnLeaveWay() && this.CurrentFlowFlag != eFlowFlag.�ȴ�����
					&& this.CurrentImperfectCar != null) ResetGoods();
			}
			catch (Exception ex)
			{
				Log4Neter.Error("timer_Goods_Tick", ex);
			}
			finally
			{
				timer_Goods.Start();
			}
		}

		/// <summary>
		/// ��ȡδ��ɵ��������ʼ�¼
		/// </summary>
		void LoadTodayUnFinishGoodsTransport()
		{
			superGridControl1_Goods.PrimaryGrid.DataSource = weighterDAO.GetUnFinishGoodsTransport();
		}

		/// <summary>
		/// ��ȡָ����������ɵ��������ʼ�¼
		/// </summary>
		void LoadTodayFinishGoodsTransport()
		{
			superGridControl2_Goods.PrimaryGrid.DataSource = weighterDAO.GetFinishedGoodsTransport(DateTime.Now.Date, DateTime.Now.Date.AddDays(1));
		}

		#endregion

		#region ��������

		Font directionFont = new Font("΢���ź�", 16);

		Pen redPen1 = new Pen(Color.Red, 1);
		Pen greenPen1 = new Pen(Color.Lime, 1);
		Pen redPen3 = new Pen(Color.Red, 3);
		Pen greenPen3 = new Pen(Color.Lime, 3);

		/// <summary>
		/// ��ǰ�Ǳ�����������
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void panCurrentWeight_Paint(object sender, PaintEventArgs e)
		{
			try
			{
				PanelEx panel = sender as PanelEx;

				int height = 12;

				// ���Ƶظ�1
				e.Graphics.DrawLine(this.InductorCoil1 ? redPen3 : greenPen3, 15, 1, 15, height);
				e.Graphics.DrawLine(this.InductorCoil1 ? redPen3 : greenPen3, 15, panel.Height - height, 15, panel.Height - 1);

				// ���Ƶظ�2
				e.Graphics.DrawLine(this.InductorCoil2 ? redPen3 : greenPen3, panel.Width - 15, 1, panel.Width - 15, height);
				e.Graphics.DrawLine(this.InductorCoil2 ? redPen3 : greenPen3, panel.Width - 15, panel.Height - height, panel.Width - 15, panel.Height - 1);

				//// ���Ƶظ�3
				//e.Graphics.DrawLine(this.InductorCoil3 ? redPen3 : greenPen3, 25, 1, 25, height);
				//e.Graphics.DrawLine(this.InductorCoil3 ? redPen3 : greenPen3, 25, panel.Height - height, 25, panel.Height - 1);

				//// ���Ƶظ�4
				//e.Graphics.DrawLine(this.InductorCoil4 ? redPen3 : greenPen3, panel.Width - 25, 1, panel.Width - 25, height);
				//e.Graphics.DrawLine(this.InductorCoil4 ? redPen3 : greenPen3, panel.Width - 25, panel.Height - height, panel.Width - 25, panel.Height - 1);

				// ���ƶ���1
				e.Graphics.DrawLine(this.InfraredSensor1 ? redPen1 : greenPen1, 35, 1, 35, height);
				e.Graphics.DrawLine(this.InfraredSensor1 ? redPen1 : greenPen1, 35, panel.Height - height, 35, panel.Height - 1);

				// ���ƶ���2
				e.Graphics.DrawLine(this.InfraredSensor2 ? redPen1 : greenPen1, panel.Width / 2, 1, panel.Width / 2, height);
				e.Graphics.DrawLine(this.InfraredSensor2 ? redPen1 : greenPen1, panel.Width / 2, panel.Height - height, panel.Width / 2, panel.Height - 1);

				// ���ƶ���3
				e.Graphics.DrawLine(this.InfraredSensor3 ? redPen1 : greenPen1, panel.Width - 35, 1, panel.Width - 35, height);
				e.Graphics.DrawLine(this.InfraredSensor3 ? redPen1 : greenPen1, panel.Width - 35, panel.Height - height, panel.Width - 35, panel.Height - 1);

				// �ϰ�����
				eDirection direction = this.CurrentDirection;
				if (this.Direction != "���ϰ�")
				{
					e.Graphics.DrawString("��>", directionFont, direction == eDirection.Way1 ? Brushes.Red : Brushes.Lime, 2, 17);
				}
				if (this.Direction != "���ϰ�")
				{
					e.Graphics.DrawString("<��", directionFont, direction == eDirection.Way2 ? Brushes.Red : Brushes.Lime, panel.Width - 47, 17);
				}
			}
			catch (Exception ex)
			{
				Log4Neter.Error("panCurrentCarNumber_Paint�쳣", ex);
			}
		}

		private void superGridControl_BeginEdit(object sender, DevComponents.DotNetBar.SuperGrid.GridEditEventArgs e)
		{
			// ȡ������༭
			e.Cancel = true;
		}

		/// <summary>
		/// �����к�
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void superGridControl_GetRowHeaderText(object sender, DevComponents.DotNetBar.SuperGrid.GridGetRowHeaderTextEventArgs e)
		{
			e.Text = (e.GridRow.RowIndex + 1).ToString();
		}

		/// <summary>
		/// Invoke��װ
		/// </summary>
		/// <param name="action"></param>
		public void InvokeEx(Action action)
		{
			if (this.IsDisposed || !this.IsHandleCreated) return;

			this.Invoke(action);
		}

		#endregion

	}
}
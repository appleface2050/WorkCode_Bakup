using System;
using System.Globalization;

namespace BlueStacks.hyperDroid.LogCollector {

public static class Strings {

	private static string s_Locale = CultureInfo.CurrentCulture.Name;

	public static String NC_ONLY_FORM_TEXT {
		get
		{
			switch(s_Locale)
			{
				case "zh-CN":
					return "通知中心支持工具";
					
				default:
					return "Notification Center Support Tool";
					
			}	
		}
	}
	public static String FORM_TEXT
	{
		get
		{
			switch(s_Locale)
			{
				case "zh-CN":
					return "模拟器支持工具";
					
				default:
					return "App Player Support Tool";
					
			}	
		}
	} 

	public static String BUTTON_TEXT
	{
		get
		{
			switch(s_Locale)
			{
				case "zh-CN":
					return "发送";
					
				default:
					return "Send";
					
			}
		}
	}

	public static String EMAIL_LABEL
	{
		get
		{
			switch(s_Locale)
			{
				case "zh-CN":
					return "电子邮件地址";
					
				default:
					return "Email Address";
					
			}
		}
	}

	public static String ZENDESK_ID_TEXT
	{
		get
		{
			switch(s_Locale)
			{
				case "zh-CN":
					return "电子邮件地址";
					
				default:
					return "Zendesk Id (Optional)";
					
			}
		}
	}

	public static String DESCRIPTION_LABEL
	{
		get
		{
			switch(s_Locale)
			{
				case "zh-CN":
					return "问题描述";
					
				default:
					return "Problem Description";
					
			}
		}
	}

	public static String STATUS_INITIAL
	{
		get
		{
			switch(s_Locale)
			{
				case "zh-CN":
					return "开始收集信息";
					
				default:
					return "Starting Collector";
					
			}
		}
	}
	public static String STATUS_COLLECTING_PRODUCT
	{
		get
		{
			switch(s_Locale)
			{
				case "zh-CN":
					return "正在收集产品信息";
					
				default:
					return "Collecting Product Information";
					
			}
		}
	}
	public static String STATUS_COLLECTING_HOST 
	{
		get
		{
			switch(s_Locale)
			{
				case "zh-CN":
					return "正在收集主机信息";
					
				default:
					return "Collecting Host Information";
					
			}
		}
	}

	public static String STATUS_COLLECTING_GUEST 
	{
		get
		{
			switch(s_Locale)
			{
				case "zh-CN":
					return "正在收集安卓的相关信息";
					
				default:
					return "Collecting Android Information";
					
			}
		}
		
	}

	public static String STATUS_ARCHIVING 
	{
		get
		{
			switch(s_Locale)
			{
				case "zh-CN":
					return "正在打包报告";
					
				default:
					return "Creating Support Archive";
					
			}
		}
	}
		
	public static String STATUS_SENDING 
	{
		get
		{
			switch(s_Locale)
			{
				case "zh-CN":
					return "正在发送问题报告";
					
				default:
					return "Sending Problem Report";
					
			}
		}
	}

	public static String APP_NAME
	{
		get
		{
			switch(s_Locale)
			{
				case "zh-CN":
					return "输入应用名称";
					
				default:
					return "Enter App Name";
					
			}
		}
	}

	public static String FINISH_CAPT 
	{ 
		get
		{
			switch(s_Locale)
			{
				case "zh-CN":
					return "支持工具已完成";
					
				default:
					return "Support Tool Complete";;
					
			}
		}
	}

	public static String FINISH_TEXT 
	{
		get
		{
			switch(s_Locale)
			{
				case "zh-CN":
					return "您的问题报告已经发送成功，我们会尽快回复您。";
					
				default:
					return "Your problem report has been sent. We will get back to you soon.";
					
			}
		}
		
	}

	public static String PROMPT_TEXT 
	{
		get
		{
			switch(s_Locale)
			{
				case "zh-CN":
					return "问题报告发送失败。 一个名叫BlueStacks-Support.zip的文件已经生成并放在您的桌面上。请把这个文件发送到support@bluestacks.com， 并附上针对您问题的简单描述。";
					
				default:
					return "Could not send Problem Report. " +
						"A file named BlueStacks-Support.zip has been created on " +
						"your desktop. " +
						"Please e-mail this file along with a brief description of " +
						"your issue to support@bluestacks.com.";;
					
			}
		}
	}

	public static String DESC_MISSING_TEXT 
	{
		get
		{
			switch(s_Locale)
			{
				case "zh-CN":
					return "请描述您遇到的问题。";
					
				default:
					return "Please enter a description of your problem.";
					
			}
		}
		
	}

	public static String ZENDESK_INVALID_ID_TEXT 
	{
		get
		{
			switch(s_Locale)
			{
				case "zh-CN":
					return "请输出一个合法的zendesk ID";
					
				default:
					return "Please enter a valid zendesk ID";
					
			}
		}
		
	}

	public static String SELECT_CATEGORY_TEXT 
	{
		get
		{
			switch(s_Locale)
			{
				case "zh-CN":
					return "请选择问题的分类。";
					
				default:
					return "Please select a problem category.";
					
			}
		}
	}

	public static String EMAIL_MISSING_TEXT 
	{
		get
		{
			switch(s_Locale)
			{
				case "zh-CN":
					return "错误的邮件地址！请输出一个正确的邮件地址";
					
				default:
					return "Invalid Email. Please Enter a valid email address.";
					
			}
		}
	}

	public static String RPC_FORM_TEXT
	{
		get
		{
			switch(s_Locale)
			{
				case "zh-CN":
					return "处理RPC错误";

				default:
					return "Troubleshoot RPC Error";

			}
		}
	}

	public static String RPC_WORK_DONE_TEXT
	{
		get
		{
			switch(s_Locale)
			{
				case "zh-CN":
					return "任务完成。如果这个问题仍然存在，请回报问题。";

				default:
					return "All Done, If the issue still persists then please report a problem";

			}
		}
	}

	public static String RPC_PROGRESS_Text
	{
		get
		{
			switch(s_Locale)
			{
				case "zh-CN":
					return "请稍后，我们正在处理这个问题。";

				default:
					return "Please wait, we are trying to fix the issue";

			}
		}
	}

	public static String RPC_TROUBLESHOOTER_TEXT 
	{
		get
		{
			switch(s_Locale)
			{
				case "zh-CN":
					return "处理并尝试自动解决我的问题";

				default:
					return "Troubleshoot and try to fix my problem automatically";

			}
		}
	}

	public static String LOGCOLLECTOR_RUNNING_TEXT 
	{
		get
		{
			switch(s_Locale)
			{
				case "zh-CN":
					return "日志收集器已经在运行";

				default:
					return "Report Problem already running";

			}
		}
	}

	public static String[] PROBLEMS
	{
		get
		{
			switch(s_Locale)
			{
				case "zh-CN":
					return new String[] {
						"分类",
						"BlueStacks在初始化的时候卡住",
						"BlueStacks崩溃",
						"系统重启/ 崩溃",
						"应用无法运行",
						"其它"
					};

				default:
					return new String[] {
						"Category",
						"Bluestacks stuck at initializing",
						"Bluestacks crash",
						"System restart/crash",
						"App not working",
						"Other"
					};

			}
		}

		
	}



	public static String APP_MISSING_TEXT 
	{
		get
		{
			switch(s_Locale)
			{
				case "zh-CN":
					return "输入应用名称";
					
				default:
					return "Enter App Name";
					
			}
		}
	}


	public static String ZIP_NAME 
	{ 
		get
		{
			return "BlueStacks-Support.zip";
		}
	}
}
}

using System;
using System.Reflection;
using System.Collections;
using System.Text;

namespace BlueStacks.hyperDroid.Common
{
	public class GetOpt
	{
		[AttributeUsage(AttributeTargets.Field)]
		public class Arg : Attribute
		{
			protected String mName;
			protected Object mValue;
			protected String mDescription;

			public String Name
			{
				get { return mName; }
				set { mName = value; }
			}

			public Object Value
			{
				get { return mValue; }
				set { mValue = value; }
			}

			public String Description
			{
				get { return mDescription; }
				set { mDescription = value; }
			}
		}

		public void Parse(String[] args)
		{
			int n = 0;

			while (n < args.Length)
			{
				int pos = OptionPos(args[n]);
				if (pos > 0)
				{
					if (GetOption(args, ref n, pos))
						mCount++;
					else
						InvalidOption(args[Math.Min(n, args.Length - 1)]);
				}
				else
				{
					if (mArgs == null)
						mArgs = new ArrayList();
					mArgs.Add(args[n]);
					if (!IsValidArg(args[n]))
						InvalidOption(args[n]);
				}
				++n;
			}
		}

		public IList InvalidArgs
		{
			get { return mInvalidArgs; }
		}

		public bool NoArgs
		{
			get { return ArgCount == 0 && mCount == 0; }
		}

		public int ArgCount
		{
			get { return mArgs == null ? 0 : mArgs.Count; }
		}

		public bool IsInValid
		{
			get { return mIsInvalid; }
		}


		protected virtual int OptionPos(String opt)
		{
			char[] c = null;

			if (opt.Length < 2)
				return 0;

			if (opt.Length > 2)
			{
				c = opt.ToCharArray(0, 3);
				if (c[0] == '-' && c[1] == '-' && IsOptionNameChar(c[2]))
					return 2;
			}
			else
			{
				c = opt.ToCharArray(0, 2);
			}

			if (c[0] == '-' && IsOptionNameChar(c[1]))
				return 1;

			return 0;
		}

		protected virtual bool IsOptionNameChar(char c)
		{
			return Char.IsLetterOrDigit(c) || c == '?';
		}

		protected virtual void InvalidOption(String name)
		{
			mInvalidArgs.Add(name);
			mIsInvalid = true;
		}

		protected virtual bool IsValidArg(String arg)
		{
			return true;
		}

		protected virtual bool MatchName(FieldInfo field, String name)
		{
			Object[] args = field.GetCustomAttributes(typeof(Arg), true);
			foreach (Arg arg in args)
			{
				if (String.Compare(arg.Name, name, true) == 0)
					return true;
			}

			return false;
		}

		protected virtual FieldInfo GetMemberField(String name)
		{
			Type t = this.GetType();
			FieldInfo[] fields = t.GetFields(BindingFlags.Instance | BindingFlags.Public);
			foreach (FieldInfo field in fields)
			{
				if (string.Compare(field.Name, name, true) == 0)
					return field;

				if (MatchName(field, name))
					return field;
			}
			return null;
		}

		protected virtual Object GetOptionValue(FieldInfo field)
		{
			Object[] atts = field.GetCustomAttributes(typeof(Arg), true);
			foreach (Object att in atts)
			{
				Console.WriteLine(att);

			}

			if (atts.Length > 0)
			{
				Arg att = (Arg)atts[0];
				return att.Value;
			}
			return null;
		}

		protected virtual bool GetOption(String[] args, ref int index, int pos)
		{
			try
			{
				Object cmdLineVal = null;
				String opt = args[index].Substring(pos, args[index].Length - pos);
				//Console.Write("index:"+index+"  ");
				SplitOptionAndValue(ref opt, ref cmdLineVal);
				FieldInfo field = GetMemberField(opt);

				if (field != null)
				{
					Object value = GetOptionValue(field);
					if (value == null)
					{
						if (field.FieldType == typeof(bool))
							value = true;

						else if (field.FieldType == typeof(string))
						{
							value = cmdLineVal != null ? cmdLineVal : args[++index];
							field.SetValue(this, Convert.ChangeType(value, field.FieldType));
							string stringValue = (string)value;
							if (stringValue == null || stringValue.Length == 0)
								return false;

							return true;
						}
						else if (field.FieldType.IsEnum)
							value = Enum.Parse(field.FieldType, (string)cmdLineVal, true);
						else
							value = cmdLineVal != null ? cmdLineVal : args[++index];
					}

					field.SetValue(this, Convert.ChangeType(value, field.FieldType));
					return true;
				}
			}
			catch (Exception)
			{
			}
			return false;
		}

		protected virtual void SplitOptionAndValue(ref String opt, ref Object val)
		{
			int pos = opt.IndexOfAny(new char[] { ':', '=' });
			//Console.WriteLine("pos:"+pos+"  ");

			if (pos < 1) return;

			val = opt.Substring(pos + 1);
			opt = opt.Substring(0, pos);
		}

		public virtual void Help()
		{
			Console.WriteLine(GetHelpText());
		}

		public virtual string GetHelpText()
		{
			StringBuilder helpText = new StringBuilder();

			Type t = this.GetType();
			FieldInfo[] fields = t.GetFields(BindingFlags.Instance | BindingFlags.Public);

			char optChar = '-';
			foreach (FieldInfo field in fields)
			{
				Object[] atts = field.GetCustomAttributes(typeof(Arg), true);
				if (atts.Length > 0)
				{
					Arg arg = (Arg)atts[0];
					if (arg.Description != null)
					{
						string valType = "";
						if (arg.Value == null)
						{
							if (field.FieldType == typeof(int))
								valType = "[Integer]";
							else if (field.FieldType == typeof(float))
								valType = "[Float]";
							else if (field.FieldType == typeof(string))
								valType = "[String]";
							else if (field.FieldType == typeof(bool))
								valType = "[Boolean]";
						}

						helpText.AppendFormat("{0}{1,-20}\n\t{2}",
							optChar, field.Name + valType, arg.Description);
						if (arg.Name != null)
							helpText.AppendFormat(" (Name format: {0}{1}{2})",
								optChar, arg.Name, valType);
						helpText.Append(Environment.NewLine);
					}
				}
			}
			return helpText.ToString();
		}
		protected ArrayList mArgs;
		protected bool mIsInvalid = false;

		public int mCount;
		private ArrayList mInvalidArgs = new ArrayList();
	}
}

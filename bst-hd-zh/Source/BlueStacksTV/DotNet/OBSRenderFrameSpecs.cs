using System;
using CodeTitans.JSon;
using BlueStacks.hyperDroid.Common;

namespace BlueStacks.hyperDroid.BlueStacksTV
{
	public class OBSRenderFrameSpecs
	{
		public int xPosition = -1;
		public int yPosition = 73;
		public int width = 280;
		public int height = 158;
		public int widthRatio;
		public int heightRatio;
		public bool preserveAspectRatio = true;

		public OBSRenderFrameSpecs(string jsonString)
		{
			JSonReader jsonReader = new JSonReader();
			IJSonObject obj = jsonReader.ReadAsJSonObject(jsonString);

			if (obj.Contains("xPosition"))
				xPosition = Convert.ToInt32(obj["xPosition"].StringValue);
			if (obj.Contains("yPosition"))
				yPosition = Convert.ToInt32(obj["yPosition"].StringValue);
			if (obj.Contains("width"))
				width = Convert.ToInt32(obj["width"].StringValue);
			if (obj.Contains("height"))
				height = Convert.ToInt32(obj["height"].StringValue);

			string network = "twitch";
			if (obj.Contains("network"))
				network = obj["network"].StringValue;

			if (network.Equals("facebook"))
			{
				widthRatio = 1;
				heightRatio = 1;
			}
			else
			{
				widthRatio = 16;
				heightRatio = 9;
			}

			if (obj.Contains("widthRatio"))
				widthRatio = Convert.ToInt32(obj["widthRatio"].StringValue);
			if (obj.Contains("heightRatio"))
				heightRatio = Convert.ToInt32(obj["heightRatio"].StringValue);
			if (obj.Contains("preserveAspectRatio"))
				preserveAspectRatio = Convert.ToBoolean(obj["preserveAspectRatio"].StringValue);
		}
	}
}

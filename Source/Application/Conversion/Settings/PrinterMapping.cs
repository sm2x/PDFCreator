using pdfforge.DataStorage.Storage;
using pdfforge.DataStorage;
using PropertyChanged;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System;

// ! This file is generated automatically.
// ! Do not edit it outside the sections for custom code.
// ! These changes will be deleted during the next generation run

namespace pdfforge.PDFCreator.Conversion.Settings
{
	[ImplementPropertyChanged]
	public partial class PrinterMapping : INotifyPropertyChanged {
		#pragma warning disable 67
		public event PropertyChangedEventHandler PropertyChanged;
		#pragma warning restore 67
		
		
		public string PrinterName { get; set; } = "";
		
		public string ProfileGuid { get; set; } = "";
		
		
		public void ReadValues(Data data, string path) {
			try { PrinterName = Data.UnescapeString(data.GetValue(@"" + path + @"PrinterName")); } catch { PrinterName = "";}
			try { ProfileGuid = Data.UnescapeString(data.GetValue(@"" + path + @"ProfileGuid")); } catch { ProfileGuid = "";}
		}
		
		
		public void StoreValues(Data data, string path) {
			data.SetValue(@"" + path + @"PrinterName", Data.EscapeString(PrinterName));
			data.SetValue(@"" + path + @"ProfileGuid", Data.EscapeString(ProfileGuid));
		}
		
		public PrinterMapping Copy()
		{
			PrinterMapping copy = new PrinterMapping();
			
			copy.PrinterName = PrinterName;
			copy.ProfileGuid = ProfileGuid;
			
			return copy;
		}
		
		public override bool Equals(object o)
		{
			if (!(o is PrinterMapping)) return false;
			PrinterMapping v = o as PrinterMapping;
			
			if (!PrinterName.Equals(v.PrinterName)) return false;
			if (!ProfileGuid.Equals(v.ProfileGuid)) return false;
			
			return true;
		}
		
		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();
			
			sb.AppendLine("PrinterName=" + PrinterName.ToString());
			sb.AppendLine("ProfileGuid=" + ProfileGuid.ToString());
			
			return sb.ToString();
		}
		
		public override int GetHashCode()
		{
			// ReSharper disable once BaseObjectGetHashCodeCallInGetHashCode
			return base.GetHashCode();
		}
		
// Custom Code starts here
// START_CUSTOM_SECTION:GENERAL

	    public PrinterMapping()
	    {
	        
	    }

        public PrinterMapping(string printerName, string profileGuid)
        {
            PrinterName = printerName;
            ProfileGuid = profileGuid;
        }

// END_CUSTOM_SECTION:GENERAL
// Custom Code ends here. Do not edit below
		
	}
}

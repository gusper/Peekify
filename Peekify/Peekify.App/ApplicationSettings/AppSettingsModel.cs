using System.Runtime.Serialization;

namespace Peekify.App.ApplicationSettings
{
	[DataContract]
    public class AppSettingsModel
    {
        [DataMember]
        public int WindowLocationLeft { get; set; }

        [DataMember]
        public int WindowLocationTop { get; set; } // No longer used, but keeping just in case
    }
}

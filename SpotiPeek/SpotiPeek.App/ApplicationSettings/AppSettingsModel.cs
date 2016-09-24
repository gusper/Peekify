using System.Runtime.Serialization;

namespace SpotiPeek.App.ApplicationSettings
{
	[DataContract]
    public class AppSettingsModel
    {
        [DataMember]
        public int WindowLocationLeft { get; set; }

        [DataMember]
        public int WindowLocationTop { get; set; }
    }
}

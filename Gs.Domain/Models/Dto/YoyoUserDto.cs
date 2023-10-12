namespace Gs.Domain.Models.Dto
{
    public class YoyoUserDto
    {
        public string Mobile { get; set; }
        public string Password { get; set; }
        public string UniqueID { get; set; }
        public string SystemName { get; set; }
        public string SystemVersion { get; set; }
        public string DeviceName { get; set; }
        public string Version { get; set; }
        public string UserPic { get; set; }
        public decimal Lat { get; set; }
        public decimal Lng { get; set; }
        public string Province { get; set; }
        public string ProvinceCode { get; set; }
        public string City { get; set; }
        public string CityCode { get; set; }
        public string Area { get; set; }
        public string AreaCode { get; set; }
    }
}
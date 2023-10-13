namespace ErrorDataLayer
{
    public class ErrorEntity
    {
        public byte[] ImageV { get; set; }
        
        public byte[] ImageM{ get; set; }
        
        public int Id { get; set; }
       
        
        public string Comment { get; set; }
        
        public string Position { get; set; }
        
        public string RouteName { get; set; }
        
        public string TimeStamp { get; set; }
        
        public string User { get; set; }
        
        public string ErrorType { get; set; }
        
        public string Priority { get; set; }
    }
}
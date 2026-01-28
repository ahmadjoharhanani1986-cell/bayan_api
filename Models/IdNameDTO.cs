using System;

namespace SHLAPI.Models
{
    public class IdNameDTO
    {
        public int id { get; set; }
        public string name { get; set; }
        public int status { get; set; }
        public string status_caption { get; set; }
        public int development_type { get; set; }

    }

    public class StoryIdNameDTOs
    {
        public int id { get; set; }
        public string name { get; set; }
        public int status { get; set; }
        public int be_developer_tl_id { get; set; }
        public int fe_developer_tl_id { get; set; }
        public int be_developer_id { get; set; }
        public int fe_developer_id { get; set; }
        public int development_type { get; set; }
        public int qa_tl_id { get; set; }

    }    
}
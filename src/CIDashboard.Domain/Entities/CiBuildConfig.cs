﻿namespace CIDashboard.Domain.Entities
{
    public class CiBuildConfig
    {
        public CiSource CiSource { get; set; }

        public string Id { get; set; }

        public string Name { get; set; }

        public string Url { get; set; }

        public string ProjectName { get; set; }
    }
}

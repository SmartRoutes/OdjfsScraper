﻿using System;

namespace OdjfsScraper.Models
{
    public class ChildCare
    {
        public int Id { get; set; }
        public DateTime LastScrapedOn { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public DateTime? LastGeocodedOn { get; set; }

        public int CountyId { get; set; }
        public County County { get; set; }

        public string ExternalUrlId { get; set; }
        public string ExternalId { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public int ZipCode { get; set; }
        public string PhoneNumber { get; set; }
    }
}
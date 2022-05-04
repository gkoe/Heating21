using Base.Entities;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Entities
{
    public class Measurement : EntityObject
    {
        public double Value { get; set; }
        public DateTime Time { get; set; }

        [ForeignKey(nameof(ItemId))]
        public Item Item { get; set; }
        public int ItemId { get; set; }


        public override string ToString()
        {
            return $"{Time.ToShortTimeString()}: {Value}";
        }
    }
}

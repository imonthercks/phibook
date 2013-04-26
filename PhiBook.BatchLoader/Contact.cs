using System;
using FileHelpers;

namespace PhiBook.BatchLoader
{
    [DelimitedRecord(","), IgnoreEmptyLines, ]
    public class Contact
    {
        public string Id;

        public string Status;

        public string BadAddress;
        
        public string Email;
        
        public string Email2;
        
        public string FirstName;
        
        public string MiddleInitial;

        [FieldQuoted(QuoteMode.OptionalForBoth)]
        public string LastName;

        [FieldConverter(ConverterKind.Date, "M/d/yyyy")]
        public DateTime? InitiationDate;
        
        public string GraduationYear;
        
        [FieldConverter(ConverterKind.Date, "M/d/yyyy")]
        public DateTime? DateOfDeath;
        
        [FieldQuoted(QuoteMode.OptionalForBoth)]
        public string Address1;

        [FieldQuoted(QuoteMode.OptionalForBoth)]
        public string Address2;
        
        public string City;
        
        public string State;
        
        public string PostalCode;

        public string Country;
        
        public string WorkPhone;
        
        public string HomePhone;
        
        [FieldOptional]
        public string MobilePhone;

        [FieldOptional, FieldConverter(ConverterKind.Date, "M/d/yyyy")]
        public DateTime? Updated;

        // Takes care of extra comma at the end of each record
        [FieldOptional]
        public string Dummy;

    }
}

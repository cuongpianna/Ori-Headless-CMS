//******************************************************************************
// <copyright file="license.md" company="RawCMS project  (https://github.com/arduosoft/RawCMS)">
// Copyright (c) 2019 RawCMS project  (https://github.com/arduosoft/RawCMS)
// RawCMS project is released under GPL3 terms, see LICENSE file on repository root at  https://github.com/arduosoft/RawCMS .
// </copyright>
// <author>Daniele Fontani, Emanuele Bucarelli, Francesco Mina'</author>
// <autogenerated>true</autogenerated>
//******************************************************************************
namespace RawCMS.Library.Schema.Validation
{
    public class NumberValidation : BaseJavascriptValidator
    {
        public override string Type => "number";

        public override string Javascript
        {
            get
            {
                return @"
const innerValidation = function() {
    if (value === null || value === undefined) {
        return;
    }

    // code starts here
    floatVal = parseFloat(value);

    if (isNaN(floatVal) || floatVal  === NaN ) {
        errors.push({""Code"":""FLOAT-01"", ""Title"":""Not a number""});
        return;
    }

    if (options.min !== undefined && options.min > floatVal) {
        errors.push({""Code"":""FLOAT-02"", ""Title"":""less than minimum"",""Description"":""ddd""});
    }

    if (options.max !== undefined && options.max < floatVal)
    {
        errors.push({""Code"":""FLOAT-03"", ""Title"":""greater than max"",""Description"":""ddd""});
    }

    return JSON.stringify(errors);
};

var backendResult = innerValidation();
            ";
            }
        }
    }
}
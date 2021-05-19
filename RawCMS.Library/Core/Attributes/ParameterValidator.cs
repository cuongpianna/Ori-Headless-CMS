﻿//******************************************************************************
// <copyright file="license.md" company="RawCMS project  (https://github.com/arduosoft/RawCMS)">
// Copyright (c) 2019 RawCMS project  (https://github.com/arduosoft/RawCMS)
// RawCMS project is released under GPL3 terms, see LICENSE file on repository root at  https://github.com/arduosoft/RawCMS .
// </copyright>
// <author>Daniele Fontani, Emanuele Bucarelli, Francesco Mina'</author>
// <autogenerated>true</autogenerated>
//******************************************************************************
using Microsoft.AspNetCore.Mvc.Filters;
using System.Net;
using System.Text.RegularExpressions;

namespace RawCMS.Library.Core.Attributes
{
    public class ParameterValidator : ActionFilterAttribute
    {
        private readonly string name;
        private readonly string regexp;
        private readonly bool negate;

        public ParameterValidator(string name, string regexp, bool negate)
        {
            this.name = name;
            this.regexp = regexp;
            this.negate = negate;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            bool match = Regex.IsMatch(context.RouteData.Values[name] as string, regexp);
            if (negate)
            {
                match = !match;
            }
            if (!match)
            {
                context.Result = new SendStatusCode(HttpStatusCode.Forbidden);
                return;
            }
            else
            {
                base.OnActionExecuting(context);
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraphQL.HotChocolate
{
    public class ErrorFilter : IErrorFilter
    {
        public IError OnError(IError error)
        {
            if (error.Exception is AggregateException ex)
            {
                var errors = new List<IError>();

                foreach (Exception innerException in ex.InnerExceptions)
                {
                    errors.Add(error.WithMessage(innerException.Message).WithException(innerException));
                }

                return new AggregateError(errors);
            }

            return error;
        }
    }
}

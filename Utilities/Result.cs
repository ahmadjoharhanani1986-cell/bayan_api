using System.Collections.Generic;

namespace SHLAPI
{
    public class ErrorDescription
    {
        public int id { get; set; }

        public string description { get; set; }
    }

    public class Result
    {
        public bool isSucceeded { get; set; }

        public ErrorDescription mainError { get; set; }

        public List<ErrorDescription> errorsList { get; set; }

        public object dataObject { get; set; }

        public Result()
        {
            isSucceeded = true;
            mainError = null;
            errorsList = new List<ErrorDescription>();
        }

        public Result(bool _isSucceeded)
        {
            isSucceeded = _isSucceeded;
            mainError = null;
            errorsList = new List<ErrorDescription>();
        }

        public static Result operator +(Result r1, Result r2)
        {
            if (!r2.isSucceeded) r1.isSucceeded = false;
            if(r1.mainError==null) r1.mainError=r2.mainError;
            foreach (ErrorDescription err in r2.errorsList)
                r1.errorsList.Add (err);
            return r1;
        }

        public static Result Fail_(string err)
        {
            Result r=new Result();
            return r.Fail(err);
        }

        public Result Fail(string err)
        {
            ErrorDescription error=new ErrorDescription() { description = err, id = 0 };
            if (mainError == null)
                mainError = error;
            errorsList.Add(error);
            isSucceeded = false;
            return this; 
        }

        public Result Fail(params string[] errs)
        {
            return Fail(string.Join(' ',errs));
        }

        public Result Fail(ErrorDescription err)
        {
            if (mainError == null)mainError=err;
            errorsList.Add(err);
            isSucceeded = false;
            return this;
        }
    }
}

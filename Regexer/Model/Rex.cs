using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using Regexer.Model;

namespace Regexer.Model
{
    public enum RegexType
    {
        Unknown,
        IsMatch,
        Match,
        Matches,
    }



    public interface IRex
    {
        void SetPattern(string pattern);

        bool Match(string input);

        Task<bool> MatchAsync(string input);

        string GetResult();

        string GetReplaceResult(string replaceFormat);

        string GetInfomation();

        string Pattern { get; }

        RegexType RegexType { get; }

        bool NeedUpdate(RegexType type, string pattern, RegexOptions options);
    }

    abstract public class RexBase :
        IRex
    {
        public RexBase(string pattern, RegexOptions options)
        {
            if (!string.IsNullOrEmpty(pattern))
            {
                this.regex = new Regex(pattern, options);
            }
        }

        protected Regex regex;

        protected bool isExec;

        public string Pattern { get; protected set; }

        public RegexType RegexType { get; protected set; }

        abstract public bool Match(string input);

        abstract public string GetResult();
        
        abstract public string GetReplaceResult(string replaceFormat);

        abstract public string GetInfomation();

        public async Task<bool> MatchAsync(string input)
        {
            var result = await Task<bool>.Factory.StartNew(() =>
            {
                if (this.regex != null)
                {
                    return this.Match(input);
                }
                else
                {
                    return false;
                }

            }).ConfigureAwait(false);

            return result;
        }

        public void SetPattern(string pattern)
        {
            this.regex = new Regex(pattern, this.regex.Options);
        }

        public override bool Equals(object obj)
        {
            if (obj is RexBase rex)
            {
                return (rex.Pattern == this.Pattern) && (this.RegexType == rex.RegexType)
                    && (rex.regex.Options == this.regex.Options);
            }
            else
            {
                return false;
            }
        }

        public override int GetHashCode() => base.GetHashCode();

        public abstract bool NeedUpdate(RegexType type, string pattern, RegexOptions options);

    }

    abstract public class RexMatchBase<T> : RexBase
    {
        public RexMatchBase(string pattern, RegexOptions options)
            : base(pattern, options)
        {

        }

        protected T Result;
        protected bool isMatch;

     
        public void Reset()
        {
            this.isExec = false;
            this.Result = default(T);
        }

        public override bool NeedUpdate(RegexType type, string pattern, RegexOptions options)
        {
            bool isSameType = false;
            switch (type)
            {
                case RegexType.IsMatch:
                case RegexType.Match:
                case RegexType.Matches:
                    isSameType = true;
                    break;
                default:
                    isSameType = false;
                    break;
            }

            bool isSamePattern = (pattern == this.Pattern);

            bool isSameOptions = (this.regex == null) ? false : (options == this.regex.Options);

            return !(isSamePattern && isSameType && isSameOptions);
        }
    }

    public class RexMatch
        : RexMatchBase<Match>
    {
        public RexMatch(string pattern, RegexOptions options)
            : base(pattern, options)
        {

        }

        public override bool Match(string input)
        {
            this.isExec = false;
            this.Result = this.regex.Match(input);
            this.isMatch = this.Result.Success;
            this.isExec = true;
            return this.Result.Success;
        }


    public override string GetInfomation()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"Success:{this.Result.Success}");
            sb.AppendLine($"Index:{this.Result.Index}");
            sb.AppendLine($"Length:{this.Result.Length}");
            sb.AppendLine($"ExistNextMatch:{this.Result.NextMatch().Success}");
            sb.AppendLine($"Group Count:{this.Result.Groups.Count}");
            foreach (var group in this.Result.Groups.Cast<Group>())
            {
                sb.AppendLine($"Group Item Name:{group.Name},Index:{group.Index},Length:{group.Length},Value:{(group.Length<50?group.Value:group.Value.Substring(0,50))}");
            }

            sb.AppendLine($"Captures Count:{this.Result.Captures.Count}");

            foreach (var capture in this.Result.Captures.Cast<Capture>())
            {
                sb.AppendLine($"Group Item Index:{capture.Index},Length:{capture.Length}");
            }
            return sb.ToString();
        }

        public override string GetResult()
        {
            return this.ToString();
        }

        public override string ToString()
        {
            if (this.isExec)
            {
                if (this.Result.Success)
                {
                    return this.Result.Value;
                }
                else
                {
                    return "match failed.";
                }
            }
            else
            {
                return $"match is not done.";
            }
        }

        public override string GetReplaceResult(string replaceFormat) => this.Result.Result(replaceFormat);
    }

    public class RexIsMatch : RexMatchBase<bool>
    {
        public RexIsMatch(string pattern, RegexOptions options)
            : base(pattern, options)
        {

        }


        public override bool Match(string input)
        {
            this.isExec = false;
            this.Result = this.regex.IsMatch(input);
            this.isExec = true;
            return this.Result;
        }

        public override string GetInfomation()
        {
            return $"Success:{this.Result}";
        }

        public override string GetResult() => this.ToString();

        public override string ToString()
        {
            if (this.isExec)
            {
                if (this.Result)
                {
                    return (this.Result) ? "Success" : "Failed";
                }
                else
                {
                    return "match failed.";
                }
            }
            else
            {
                return $"match is not done.";
            }
        }

        public override string GetReplaceResult(string replaceFormat) => this.GetResult();
    }

    public class RexMatches : RexMatchBase<IList<Match>>
    {
        public RexMatches(string pattern, RegexOptions options)
                  : base(pattern, options)
        {

        }

        public override bool Match(string input)
        {
            this.isExec = false;
            var result = this.regex.Matches(input);
            this.isMatch = result.Count != 0;
            this.Result = result.Cast<Match>().ToList();
            this.isExec = true;
            return this.isMatch;
        }

        public override string GetInfomation()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"Success:{this.isMatch}");

            foreach (var match in this.Result)
            {

                sb.AppendLine($"Index:{match.Index}");
                sb.AppendLine($"Length:{match.Length}");
                sb.AppendLine($"Group Count:{match.Groups.Count}");
                foreach (var group in match.Groups.Cast<Group>())
                {
                    sb.AppendLine($"Group Item Name:{group.Name},Index:{group.Index},Length:{group.Length},Value:{(group.Length < 50 ? group.Value : group.Value.Substring(0, 50))}");
                }

                sb.AppendLine($"Captures Count:{match.Captures.Count}");

                foreach (var capture in match.Captures.Cast<Capture>())
                {
                    sb.AppendLine($"Group Item Index:{capture.Index},Length:{capture.Length}");
                }
            }
            return sb.ToString();
        }

        public override string ToString()
        {
            if (this.isExec)
            {
                if (this.isMatch)
                {
                    return "match success.";
                }
                else
                {
                    return "match failed.";
                }
            }
            else
            {
                return $"match is not done.";
            }
        }

        public override string GetResult()
        {
            if (this.isMatch)
            {
                StringBuilder sb = new StringBuilder();
                foreach(var match in this.Result)
                {
                    sb.AppendLine(match.Value);
                }
                return sb.ToString();
            }
            else
            {
                return string.Empty;
            }

        }

        public override string GetReplaceResult(string replaceFormat)
        {
            if (this.isMatch)
            {
                StringBuilder sb = new StringBuilder();
                foreach (var match in this.Result)
                {
                    sb.AppendLine(match.Result(replaceFormat));
                }
                return sb.ToString();
            }
            else
            {
                return string.Empty;
            }
        }

    }



}

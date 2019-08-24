using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace Regexer.Model
{
    public class RegexManager
    {
        private IRex Rex;


        public RegexType Type { get; private set; }

        public string Pattern { get; private set; }

        public RegexOptions Options { get; private set; }

        public string ErrorMessage { get; private set; }

        public RegexManager()
        {

        }

        public bool ExecMatch(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                this.ErrorMessage = "input is null or whitespace.";
                return false;
            }

            var result = this.Rex?.Match(input);
            if (result.HasValue)
            {
                if (!result.Value)
                {
                    this.ErrorMessage = "regex result is none.";
                }
                return result.Value;
            }
            else
            {
                this.ErrorMessage = "regex result is null.";
                return false;
            }
        }

        public string GetResult() => this.Rex?.GetResult();

        public string GetInfomation() => this.Rex?.GetInfomation();

        public async Task<bool> ExecMatchAsync(string input)
        {
            var result = await Task<bool>.Factory.StartNew(() =>
            {

                if (this.Rex != null)
                {
                    return this.ExecMatch(input);
                }
                else
                {
                    return false;
                }

            }).ConfigureAwait(false);

            return result;
        }


        public void SetRegexType(RegexType type)
        {
            this.Type = type;
            this.UpdateRegex();
        }


        public void SetPattern(string pattern)
        {
            this.Pattern = pattern;
            this.UpdateRegex();
        }

        private void UpdateRegex()
        {
            if(this.Rex==null || this.Rex.NeedUpdate(this.Type, this.Pattern,this.Options))
            {
                this.Rex = this.Creater();               
            }
        }

        private IRex Creater()
        {
            try
            {
                switch (this.Type)
                {
                    case RegexType.IsMatch:
                        return new RexIsMatch(this.Pattern, this.Options);
                    case RegexType.Match:
                        return new RexMatch(this.Pattern, this.Options);
                    case RegexType.Matches:
                        return new RexMatches(this.Pattern, this.Options);
                    case RegexType.Replce:
                        return new RexReplace(this.Pattern, this.Options);
                    case RegexType.Unknown:
                    default:
                        throw new ArgumentException();
                }
            }
            catch(Exception ex)
            {
                this.ErrorMessage = ex.Message;
                return null;
            }
        }

        public void SetMultilineOption(RegexOptions option)
        {
            this.Options = option | RegexOptions.Compiled;
            this.UpdateRegex();
        }

        public string Replace(string replaceFormat)
        {
            if (this.Rex != null)
            {
                return this.Rex.GetReplaceResult(replaceFormat);
            }
            else
            {
                return string.Empty;
            }

        }
    }
}

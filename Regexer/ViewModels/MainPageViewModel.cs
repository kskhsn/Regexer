using Prism.Commands;
using Prism.Mvvm;
using Regexer.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Text.RegularExpressions;

namespace Regexer.ViewModels
{
    public class MainPageViewModel : BindableBase
    {

        private string _RegexResult = "none";

        public string RegexResult
        {
            get { return _RegexResult; }
            private set { SetProperty(ref _RegexResult, value); }
        }

        private string _RegexText;
        public string RegexText
        {
            get { return _RegexText; }
            set
            {
                SetProperty(ref _RegexText, value);
                this.ExecRegex.RaiseCanExecuteChanged();
            }
        }

        private string _SourceText;
        public string SourceText
        {
            get { return _SourceText; }
            set { SetProperty(ref _SourceText, value); }
        }

        private string _MatchedText;
        public string MatchedText
        {
            get { return _MatchedText; }
            set { SetProperty(ref _MatchedText, value); }
        }

        private RegexManager _RegexManager;
        public RegexManager RegexManager
        {
            get { return _RegexManager; }
            set { SetProperty(ref _RegexManager, value); }
        }

        private string _MatchedResultText;
        public string MatchedResultText
        {
            get { return _MatchedResultText; }
            set { SetProperty(ref _MatchedResultText, value); }
        }

        private RegexType _SelectedRegexType;
        public RegexType SelectedRegexType
        {
            get { return _SelectedRegexType; }
            set { SetProperty(ref _SelectedRegexType, value); }
        }

        private string _RegexReplaceText;
        public string RegexReplaceText
        {
            get { return _RegexReplaceText; }
            set { SetProperty(ref _RegexReplaceText, value); }
        }


        public MainPageViewModel()
        {
            this.RegexManager = new RegexManager();
            this.SelectedRegexType = RegexType.Matches;
            this.RegexManager.SetRegexType(this.SelectedRegexType);
            this.RegexManager.SetMultilineOption(RegexOptions.Multiline);


        }

        private DelegateCommand _ExecRegex;
        public DelegateCommand ExecRegex =>
            _ExecRegex ?? (_ExecRegex = new DelegateCommand(ExecuteExecRegex, CanExecuteExecRegex));

        void ExecuteExecRegex()
        {
            this.RegexManager.SetPattern(this.RegexText);           
            this.ExecuteExecRegexCommand();
            this.RegexResult = "";
        }

        bool CanExecuteExecRegex()
        {
            return !string.IsNullOrEmpty(this.RegexText);
        }

        private DelegateCommand<string[]> _FileDropCommand;
        public DelegateCommand<string[]> FileDropCommand =>
            _FileDropCommand ?? (_FileDropCommand = new DelegateCommand<string[]>(ExecuteFileDropCommand));

        void ExecuteFileDropCommand(string[] parameter)
        {
            foreach (var item in parameter)
            {
                Console.WriteLine(item);
            }
        }

        private DelegateCommand _PasteClipBoard;
        public DelegateCommand PasteClipBoard =>
            _PasteClipBoard ?? (_PasteClipBoard = new DelegateCommand(ExecutePasteClipBoard));

        void ExecutePasteClipBoard()
        {
            this.SourceText = Clipboard.GetData(DataFormats.Text).ToString();
        }

        private DelegateCommand _CopyClipBoard;
        public DelegateCommand CopyClipBoard =>
            _CopyClipBoard ?? (_CopyClipBoard = new DelegateCommand(ExecuteCopyClipBoard));

        void ExecuteCopyClipBoard()
        {
            Clipboard.SetData(DataFormats.Text, this.MatchedText);
        }

        private DelegateCommand _ExecRegexCommand;
        public DelegateCommand ExecRegexCommand =>
            _ExecRegexCommand ?? (_ExecRegexCommand = new DelegateCommand(ExecuteExecRegexCommand));

        async void ExecuteExecRegexCommand()
        {
            try
            {
                void updateUI(bool success)
                {
                    if (success)
                    {
                        this.MatchedText = this.RegexManager.GetResult();
                        this.MatchedResultText = this.RegexManager.GetInfomation();
                    }
                    else
                    {
                        this.MatchedText = string.Empty;
                        this.MatchedResultText = "failed";
                    }
                }

                var isSuccess = await this.RegexManager.ExecMatchAsync(this.SourceText);


                var dispacher = Application.Current.Dispatcher;
                if (dispacher != null)
                {
                    dispacher.Invoke(() =>
                    {
                        updateUI(isSuccess);
                    });
                }
                else
                {
                    updateUI(isSuccess);
                }


            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }


        }

        private DelegateCommand _SelectedRegexTypeChanged;
        public DelegateCommand SelectedRegexTypeChanged =>
            _SelectedRegexTypeChanged ?? (_SelectedRegexTypeChanged = new DelegateCommand(ExecuteSelectedRegexTypeChanged));

        void ExecuteSelectedRegexTypeChanged()
        {
            this.RegexManager.SetRegexType(this.SelectedRegexType);
        }

        private DelegateCommand<RegexOptions?> _SelectedRegexLinesChanged;
        public DelegateCommand<RegexOptions?> SelectedRegexLinesChanged =>
            _SelectedRegexLinesChanged ?? (_SelectedRegexLinesChanged = new DelegateCommand<RegexOptions?>(ExecuteSelectedRegexLinesChanged));

        void ExecuteSelectedRegexLinesChanged(RegexOptions? item)
        {
            Console.WriteLine(item);
            if (item.HasValue)
            {
                this.RegexManager.SetMultilineOption(item.Value);
            }
        }

        private DelegateCommand _ExecReplace;
        public DelegateCommand ExecReplace =>
            _ExecReplace ?? (_ExecReplace = new DelegateCommand(ExecuteExecReplace, CanExecuteExecReplace));

        void ExecuteExecReplace()
        {
            try
            {
                void updateUI()
                {
                    this.MatchedText = this.RegexManager.Replace(this.RegexReplaceText);
                    this.MatchedResultText = this.RegexManager.GetInfomation();
                }

                var dispacher = Application.Current.Dispatcher;
                if (dispacher != null)
                {
                    dispacher.Invoke(() =>
                    {
                        updateUI();
                    });
                }
                else
                {
                    updateUI();
                }


            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        bool CanExecuteExecReplace()
        {
            return true;
        }

    }
}

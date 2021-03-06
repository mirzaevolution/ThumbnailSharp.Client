﻿using System;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;

namespace ThumbnailSharp.Gui.Thumbs
{
    public class ThumbViewModel:INotifyPropertyChanged
    {
        public ThumbViewModel()
        {
            ThumbModel = new ThumbModel()
            {
                ThumbnailSize = 100
            };
            CreateThumbnailCommand = new RelayCommand(OnCreateThumbnail, CanCreateThumbnail);
            TriggerOptionCommand = new RelayCommand(OnTriggerOption);
            IsLocal = true;
            IsInternet = false;
            IsEditingActive = true;
        }
        private bool _isLoading;
        private bool _isEditingActive;
        private bool _isLocal;
        private bool _isInternet;

        public ThumbModel ThumbModel { get; set; }
        public string[] Options
        {
            get
            {
                return new string[]
                {
                    "Local",
                    "Internet"
                };
            }
        }
        public string[] Formats
        {
            get
            {
                return new string[]
                {
                    "Jpeg","Bmp","Png","Gif","Tiff"
                };
            }
        }
        public bool IsLoading
        {
            get
            {
                return _isLoading;
            }
            set
            {
                if(_isLoading!=value)
                {
                    _isLoading = value;
                    OnPropertyChanged(nameof(IsLoading));
                }
            }
        }
        public bool IsEditingActive
        {
            get
            {
                return _isEditingActive;
            }
            set
            {
                if(_isEditingActive!=value)
                {
                    _isEditingActive = value;
                    OnPropertyChanged(nameof(IsEditingActive));
                }
            }
        }
        public bool IsLocal
        {
            get
            {
                return _isLocal;
            }
            set
            {
                if(_isLocal != value)
                {
                    _isLocal = value;
                    OnPropertyChanged(nameof(IsLocal));
                }
            }
        }
        public bool IsInternet
        {
            get
            {
                return _isInternet;
            }
            set
            {
                if(_isInternet!=value)
                {
                    _isInternet = value;
                    OnPropertyChanged(nameof(IsInternet));
                }
            }
        }
        public RelayCommand CreateThumbnailCommand { get; private set; }
        public RelayCommand TriggerOptionCommand { get; set; }
        public event EventHandler<string> Completed;
        public event EventHandler<string> Error;
        public event PropertyChangedEventHandler PropertyChanged;

        private void OnTriggerOption()
        {
            if (ThumbModel.Option == "Local")
            {
                IsLocal = true;
                IsInternet = false;
            }
            else
            {
                IsLocal = false;
                IsInternet = true;
            }
        }
        private async void OnCreateThumbnail()
        {
            IsEditingActive = false;
            IsLoading = true;
            if(ThumbModel.Option == "Local")
            {
                await HandleLocalRequest();
            }
            else
            {
                HandleInternetRequest();
            }
            
        }
        private bool CanCreateThumbnail()
        {
            if (String.IsNullOrEmpty(ThumbModel.Option) || 
                String.IsNullOrEmpty(ThumbModel.Location) || 
                String.IsNullOrEmpty(ThumbModel.TargetLocation) ||
                String.IsNullOrEmpty(ThumbModel.Format))
                return false;
            return true;
        }
        private async Task HandleLocalRequest()
        {
            if(!File.Exists(ThumbModel.Location))
                OnError(string.Format(LanguageResource.Instance["FileNotFoundError"], ThumbModel.Location));
            else
            {
               
                string errorDescription = LanguageResource.Instance["ErrorDescription"];
                if (!Directory.Exists(Path.GetDirectoryName(ThumbModel.TargetLocation)))
                    OnError(string.Format(LanguageResource.Instance["DirectoryNotFoundError"], Path.GetDirectoryName(ThumbModel.TargetLocation)));
                else
                {
                    Format format = GetFormat(ThumbModel.Format.ToLower());
                    bool success = false;
                    try
                    {
                        await Task.Run(async() =>
                        {
                            using (Stream sourceStream = new ThumbnailCreator().CreateThumbnailStream(ThumbModel.ThumbnailSize, ThumbModel.Location, format))
                            {
                                if (sourceStream != null)
                                {
                                    using (FileStream fs = new FileStream(ThumbModel.TargetLocation, FileMode.OpenOrCreate, FileAccess.Write))
                                    {
                                        if (sourceStream.Position != 0)
                                            sourceStream.Position = 0;

                                        await sourceStream.CopyToAsync(fs);
                                        success = true;
                                    }
                                }
                                else
                                {
                                    success = false;
                                }
                            }
                        });

                    }
                    catch (Exception ex)
                    {
                        errorDescription += $"\n{ex.Message}\n";
                        success = false;
                    }
                    finally
                    {
                        if (success)
                            OnCompleted(LanguageResource.Instance["OnCompletedSuccessful"]);
                        else
                        {
                            string message = errorDescription + "\n" + LanguageResource.Instance["OnCompletedFailed"];
                            OnCompleted(message);
                        }
                    }

                }
            }
        }
        private async void HandleInternetRequest()
        {
            if (!Uri.IsWellFormedUriString(ThumbModel.Location, UriKind.Absolute))
                OnError(LanguageResource.Instance["UrlError"]);
            
            else
            {
                string errorDescription = LanguageResource.Instance["ErrorDescription"];

                if (!Directory.Exists(Path.GetDirectoryName(ThumbModel.TargetLocation)))
                    OnError(string.Format(LanguageResource.Instance["DirectoryNotFoundError"], Path.GetDirectoryName(ThumbModel.TargetLocation)));
                else
                {
                    Format format = GetFormat(ThumbModel.Format.ToLower());
                    bool success = false;
                    try
                    {
                        using (Stream sourceStream = await new ThumbnailCreator().CreateThumbnailStreamAsync(ThumbModel.ThumbnailSize, new Uri(ThumbModel.Location,UriKind.Absolute), format))
                        {
                            if (sourceStream != null)
                            {
                                using (FileStream fs = new FileStream(ThumbModel.TargetLocation, FileMode.OpenOrCreate, FileAccess.Write))
                                {
                                    if (sourceStream.Position != 0)
                                        sourceStream.Position = 0;

                                    sourceStream.CopyTo(fs);
                                    success = true;
                                }
                            }
                            else
                            {
                                success = false;
                            }
                        }

                    }
                    catch (Exception ex)
                    {
                        errorDescription += $"\n{ex.Message}\n";
                        success = false;
                    }
                    finally
                    {
                        if (success)
                            OnCompleted(LanguageResource.Instance["OnCompletedSuccessful"]);
                        else
                        {
                            string message = errorDescription + "\n" + LanguageResource.Instance["OnCompletedFailed"];
                            OnCompleted(message);
                        }
                    }

                }
            }
        }
        private Format GetFormat(string formatStr)
        {
            Format format = Format.Jpeg;
            switch (formatStr)
            {
                case "bmp":
                    format = Format.Bmp;
                    break;
                case "png":
                    format = Format.Png;
                    break;
                case "gif":
                    format = Format.Gif;
                    break;
                case "tiff":
                    format = Format.Tiff;
                    break;
            }
            return format;
        }


        protected virtual void OnError(string error)
        {
            EventHandler<string> handler = Error;
            if(handler!=null)
            {
                handler(this, error);
            }
            IsLoading = false;
            IsEditingActive = true;
        }
        protected virtual void OnCompleted(string message)
        {
            EventHandler<string> handler = Completed;
            if (handler != null)
            {
                handler(this, message);
                ThumbModel.Location = "";
                ThumbModel.TargetLocation = "";
                ThumbModel.ThumbnailSize = 100;
            }
            IsLoading = false;
            IsEditingActive = true;
        }
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}

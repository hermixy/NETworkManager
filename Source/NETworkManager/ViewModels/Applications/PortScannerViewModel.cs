﻿using System.Windows.Input;
using System.Windows;
using System;
using System.Collections.ObjectModel;
using NETworkManager.Models.Settings;
using System.Collections.Generic;
using NETworkManager.Models.Network;
using NETworkManager.Helpers;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.Windows.Threading;
using System.Diagnostics;

namespace NETworkManager.ViewModels.Applications
{
    public class PortScannerViewModel : ViewModelBase
    {
        #region Variables
        CancellationTokenSource cancellationTokenSource;

        DispatcherTimer dispatcherTimer = new DispatcherTimer();
        Stopwatch stopwatch = new Stopwatch();

        private bool _isLoading = true;

        private string _hostnameOrIPAddress;
        public string HostnameOrIPAddress
        {
            get { return _hostnameOrIPAddress; }
            set
            {
                if (value == _hostnameOrIPAddress)
                    return;

                _hostnameOrIPAddress = value;
                OnPropertyChanged();
            }
        }

        private List<string> _hostnameOrIPAddressHistory = new List<string>();
        public List<string> HostnameOrIPAddressHistory
        {
            get { return _hostnameOrIPAddressHistory; }
            set
            {
                if (value == _hostnameOrIPAddressHistory)
                    return;

                if (!_isLoading)
                    SettingsManager.Current.PortScanner_HostnameOrIPAddressHistory = value;

                _hostnameOrIPAddressHistory = value;
                OnPropertyChanged();
            }
        }

        private string _ports;
        public string Ports
        {
            get { return _ports; }
            set
            {
                if (value == _ports)
                    return;

                _ports = value;
                OnPropertyChanged();
            }
        }

        private List<string> _portsHistory = new List<string>();
        public List<string> PortsHistory
        {
            get { return _portsHistory; }
            set
            {
                if (value == _portsHistory)
                    return;

                if (!_isLoading)
                    SettingsManager.Current.PortScanner_PortsHistory = value;

                _portsHistory = value;
                OnPropertyChanged();
            }
        }

        private bool _isScanRunning;
        public bool IsScanRunning
        {
            get { return _isScanRunning; }
            set
            {
                if (value == _isScanRunning)
                    return;

                _isScanRunning = value;
                OnPropertyChanged();
            }
        }

        private bool _cancelScan;
        public bool CancelScan
        {
            get { return _cancelScan; }
            set
            {
                if (value == _cancelScan)
                    return;

                _cancelScan = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<PortInfo> _portScanResult = new ObservableCollection<PortInfo>();
        public ObservableCollection<PortInfo> PortScanResult
        {
            get { return _portScanResult; }
            set
            {
                if (value == _portScanResult)
                    return;

                _portScanResult = value;
            }
        }

        private int _portsToScan;
        public int PortsToScan
        {
            get { return _portsToScan; }
            set
            {
                if (value == _portsToScan)
                    return;

                _portsToScan = value;
                OnPropertyChanged();
            }
        }

        private int _portsScanned;
        public int PortsScanned
        {
            get { return _portsScanned; }
            set
            {
                if (value == _portsScanned)
                    return;

                _portsScanned = value;
                OnPropertyChanged();
            }
        }

        private int _portsOpen;
        public int PortsOpen
        {
            get { return _portsOpen; }
            set
            {
                if (value == _portsOpen)
                    return;

                _portsOpen = value;
                OnPropertyChanged();
            }
        }

        private bool _preparingScan;
        public bool PreparingScan
        {
            get { return _preparingScan; }
            set
            {
                if (value == _preparingScan)
                    return;

                _preparingScan = value;
                OnPropertyChanged();
            }
        }

        private DateTime? _startTime;
        public DateTime? StartTime
        {
            get { return _startTime; }
            set
            {
                if (value == _startTime)
                    return;

                _startTime = value;
                OnPropertyChanged();
            }
        }

        private TimeSpan _duration;
        public TimeSpan Duration
        {
            get { return _duration; }
            set
            {
                if (value == _duration)
                    return;

                _duration = value;
                OnPropertyChanged();
            }
        }

        private DateTime? _endTime;
        public DateTime? EndTime
        {
            get { return _endTime; }
            set
            {
                if (value == _endTime)
                    return;

                _endTime = value;
                OnPropertyChanged();
            }
        }

        private bool _expandStatistics;
        public bool ExpandStatistics
        {
            get { return _expandStatistics; }
            set
            {
                if (value == _expandStatistics)
                    return;

                if (!_isLoading)
                    SettingsManager.Current.PortScanner_ExpandStatistics = value;

                _expandStatistics = value;
                OnPropertyChanged();
            }
        }

        private bool _displayStatusMessage;
        public bool DisplayStatusMessage
        {
            get { return _displayStatusMessage; }
            set
            {
                if (value == _displayStatusMessage)
                    return;

                _displayStatusMessage = value;
                OnPropertyChanged();
            }
        }

        private string _statusMessage;
        public string StatusMessage
        {
            get { return _statusMessage; }
            set
            {
                if (value == _statusMessage)
                    return;

                _statusMessage = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region Constructor, Load settings
        public PortScannerViewModel()
        {
            LoadSettings();

            _isLoading = false;
        }

        private void LoadSettings()
        {
            if (SettingsManager.Current.PortScanner_HostnameOrIPAddressHistory != null)
                HostnameOrIPAddressHistory = new List<string>(SettingsManager.Current.PortScanner_HostnameOrIPAddressHistory);

            if (SettingsManager.Current.PortScanner_PortsHistory != null)
                PortsHistory = new List<string>(SettingsManager.Current.PortScanner_PortsHistory);

            ExpandStatistics = SettingsManager.Current.PortScanner_ExpandStatistics;
        }
        #endregion

        #region ICommands & Actions
        public ICommand ScanCommand
        {
            get { return new RelayCommand(p => ScanAction()); }
        }

        private void ScanAction()
        {
            if (IsScanRunning)
                StopScan();
            else
                StartScan();
        }
        #endregion

        #region Methods
        private async void StartScan()
        {
            DisplayStatusMessage = false;
            StatusMessage = string.Empty;

            IsScanRunning = true;
            PreparingScan = true;

            // Measure the time
            StartTime = DateTime.Now;
            stopwatch.Start();
            dispatcherTimer.Tick += DispatcherTimer_Tick;
            dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 100);
            dispatcherTimer.Start();
            EndTime = null;

            PortScanResult.Clear();
            PortsOpen = 0;

            cancellationTokenSource = new CancellationTokenSource();

            string[] hosts = HostnameOrIPAddress.Split(';');

            List<Tuple<IPAddress, string>> hostData = new List<Tuple<IPAddress, string>>();

            for (int i = 0; i < hosts.Length; i++)
            {
                string host = hosts[i].Trim();
                string hostname = string.Empty;
                IPAddress.TryParse(host, out IPAddress ipAddress);

                try
                {
                    // Resolve DNS
                    // Try to resolve the hostname
                    if (ipAddress == null)
                    {
                        IPHostEntry ipHostEntry = await Dns.GetHostEntryAsync(host);

                        foreach (IPAddress ip in ipHostEntry.AddressList)
                        {
                            if (ip.AddressFamily == AddressFamily.InterNetwork && SettingsManager.Current.PortScanner_ResolveHostnamePreferIPv4)
                            {
                                ipAddress = ip;
                                continue;
                            }
                            else if (ip.AddressFamily == AddressFamily.InterNetworkV6 && !SettingsManager.Current.PortScanner_ResolveHostnamePreferIPv4)
                            {
                                ipAddress = ip;
                                continue;
                            }
                        }

                        // Fallback --> If we could not resolve our prefered ip protocol
                        if (ipAddress == null)
                        {
                            foreach (IPAddress ip in ipHostEntry.AddressList)
                            {
                                ipAddress = ip;
                                continue;
                            }
                        }

                        hostname = host;
                    }
                    else
                    {
                        try
                        {
                            IPHostEntry ipHostEntry = await Dns.GetHostEntryAsync(ipAddress);

                            hostname = ipHostEntry.HostName;
                        }
                        catch { }
                    }
                }
                catch (SocketException) // This will catch DNS resolve errors
                {
                    if (!string.IsNullOrEmpty(StatusMessage))
                        StatusMessage += Environment.NewLine;

                    StatusMessage += string.Format(Application.Current.Resources["String_CouldNotResolveHostnameFor"] as string, host);
                    DisplayStatusMessage = true;

                    continue;
                }

                hostData.Add(Tuple.Create(ipAddress, hostname));
            }

            if (hostData.Count == 0)
            {
                StatusMessage += Environment.NewLine + Application.Current.Resources["String_NothingToDoCheckYourInput"] as string;
                DisplayStatusMessage = true;

                ScanFinished();

                return;
            }

            int[] ports = await PortRangeHelper.ConvertPortRangeToIntArrayAsync(Ports);

            try
            {
                PortsToScan = ports.Length * hostData.Count;
                PortsScanned = 0;

                PreparingScan = false;

                HostnameOrIPAddressHistory = new List<string>(HistoryListHelper.Modify(HostnameOrIPAddressHistory, HostnameOrIPAddress, SettingsManager.Current.Application_HistoryListEntries));
                PortsHistory = new List<string>(HistoryListHelper.Modify(PortsHistory, Ports, SettingsManager.Current.Application_HistoryListEntries));

                PortScannerOptions portScannerOptions = new PortScannerOptions
                {
                    Threads = SettingsManager.Current.PortScanner_Threads,
                    ShowClosed = SettingsManager.Current.PortScanner_ShowClosed,
                    Timeout = SettingsManager.Current.PortScanner_Timeout
                };

                PortScanner portScanner = new PortScanner();
                portScanner.PortScanned += PortScanner_PortScanned;
                portScanner.ScanComplete += PortScanner_ScanComplete;
                portScanner.ProgressChanged += PortScanner_ProgressChanged;
                portScanner.UserHasCanceled += PortScanner_UserHasCanceled;

                portScanner.ScanAsync(hostData, ports, portScannerOptions, cancellationTokenSource.Token);
            }

            catch (Exception ex) // This will catch any exception
            {
                StatusMessage = ex.Message;
                DisplayStatusMessage = true;

                ScanFinished();
            }
        }

        private void StopScan()
        {
            CancelScan = true;
            cancellationTokenSource.Cancel();
        }

        private void ScanFinished()
        {
            // Stop timer and stopwatch
            stopwatch.Stop();
            dispatcherTimer.Stop();

            Duration = stopwatch.Elapsed;
            EndTime = DateTime.Now;

            stopwatch.Reset();
            
            CancelScan = false;
            IsScanRunning = false;
        }

        public void OnShutdown()
        {
            if (IsScanRunning)
                StopScan();
        }
        #endregion

        #region Events
        private void PortScanner_UserHasCanceled(object sender, EventArgs e)
        {
            StatusMessage = Application.Current.Resources["String_CanceledByUser"] as string;
            DisplayStatusMessage = true;
        }

        private void PortScanner_ProgressChanged(object sender, ProgressChangedArgs e)
        {
            PortsScanned = e.Value;
        }

        private void PortScanner_ScanComplete(object sender, EventArgs e)
        {
            ScanFinished();
        }

        private void PortScanner_PortScanned(object sender, PortScannedArgs e)
        {
            PortInfo portInfo = PortInfo.Parse(e);
                       
            Application.Current.Dispatcher.BeginInvoke(new Action(delegate ()
            {
                PortScanResult.Add(portInfo);
            }));

            if (portInfo.Status == PortInfo.PortStatus.Open)
                PortsOpen++;
        }

        private void DispatcherTimer_Tick(object sender, EventArgs e)
        {
            Duration = stopwatch.Elapsed;
        }
        #endregion               
    }
}
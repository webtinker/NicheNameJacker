using System;
using NicheNameJacker.Commands;
using NicheNameJacker.Common;
using NicheNameJacker.Common.Membership;
using NicheNameJacker.Controls;
using NicheNameJacker.Interfaces;
using NicheNameJacker.Properties;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using NicheNameJacker.Extensions;
using System.Collections.ObjectModel;

namespace NicheNameJacker.ViewModels.SubViewModels
{
    public class DomDetailerSubViewModel
    {
        private readonly object _lock = new object();

        private readonly IEnumerable<IDomDetailerStats> _items;
        private readonly IDomDetailerPermissionSet _permissions;
        private readonly string _membershipPlanName;
        private readonly Action _onStatsUpdated;

        public DomDetailerSubViewModel(IEnumerable<IDomDetailerStats> items, IDomDetailerPermissionSet permissions, string membershipPlanName, Action onStatsUpdated)
        {
            _items = items;
            _permissions = permissions;
            _membershipPlanName = membershipPlanName;
            _onStatsUpdated = onStatsUpdated;
        }

        public ICommand GetStatsCommand => new RelayCommand<IDomDetailerStats>(async d =>
        {
            if (!_permissions.CanUseDomDetailerStatsForSingle)
            {
                PermissionAssistant.ShowPermissionDeniedMessage(_membershipPlanName);
                return;
            }

            if (string.IsNullOrWhiteSpace(Settings.Default.DomDetailerKey))
            {
                if (!GetDomDetailerTrialDialogResult())
                    return;
            }

            var session = new StatsSession();
            await session.GetStats(new List<IDomDetailerStats> {d});
            
            _onStatsUpdated.Invoke();
        });

        public ICommand GetStatsForMultipleCommand => new RelayCommand(async () =>
        {
            if (!_permissions.CanUseDomDetailerStatsForMultiple)
            {
                PermissionAssistant.ShowPermissionDeniedMessage(_membershipPlanName);
                return;
            }
            
            //if (string.IsNullOrWhiteSpace(Settings.Default.DomDetailerKey))
            //{
            //    if (!GetDomDetailerTrialDialogResult())
            //        return;
            //}

            var domains = _items.Where(d => d.IsSelected  && !d.StatsLoaded ).ToList();
            foreach (var domain in domains)
            {
                domain.IsGettingStats = true;
            }

            var session = new StatsSession();
            await session.GetStats(domains);

            foreach (var domain in domains)
            {
                domain.IsGettingStats = false;
            }

            _onStatsUpdated.Invoke();
        });

        public async Task GetsStatsForDomains(IEnumerable<IDomDetailerStats> domains)
        {
           
            if (domains?.Count()==0)
                return;
            if (!_permissions.CanUseDomDetailerStatsForMultiple)
            {
                PermissionAssistant.ShowPermissionDeniedMessage(_membershipPlanName);
                return;
            }

            if (string.IsNullOrWhiteSpace(Settings.Default.DomDetailerKey))
            {
                if (!GetDomDetailerTrialDialogResult())
                    return;
            }

           
            foreach (var domain in domains)
            {
                domain.IsGettingStats = true;
            }

            var session = new StatsSession();
            await session.GetStats(domains.ToList());

            foreach (var domain in domains)
            {
                domain.IsGettingStats = false;
            }

            _onStatsUpdated.Invoke();
        }
        private bool GetDomDetailerTrialDialogResult()
        {
            lock (_lock)
            {
                if (DomDetailerTrialDialog.WasDisplayed)
                    return true;

                DomDetailerTrialDialog.WasDisplayed = true;
                return true;
                //new DomDetailerTrialDialog().ShowDialog().GetValueOrDefault();
            }
        }
    }

    class StatsSession
    {
        private bool _dialogShown;

        private readonly object _lock = new object();

        const string DefaultDomDetailerApiKey = "ADTDNE6UR3QBC";

        public async Task GetStats(List<IDomDetailerStats> domains)
        {
            var chunks = domains.ToChunks(5).ToList();

            foreach (var chunk in chunks)
            {
                try
                {
                    await chunk.Select(GetStatsForSingle).WhenAll();
                }
                catch (Exception)
                {
                    GetDomDetailerTrialDialogResult();
                    break;
                }
            }
        }

        public async Task GetStatsForSingle(IDomDetailerStats domain)
        {
            domain.IsGettingStats = true;

            //if (Settings.Default.DomDetailerKey.IsNullOrEmpty())
            //{
            //    CreditsIncrementState state = CreditsUsedManager.IncreaseUsage(domain.Address);
            //    if (state == CreditsIncrementState.QuotaExhausted)
            //    {
            //        domain.IsGettingStats = false;

            //        throw new Exception("Credits quota exhausted");
            //    }
            //}
            //no charging for dom detailer stats

            //var domDetailerData = await DomDetailerAssistant.GetDetailsAsync(domain.Address, Settings.Default.DomDetailerKey.IsNullOrEmpty()
            //    ? DefaultDomDetailerApiKey
            //    : Settings.Default.DomDetailerKey);

            var domDetailerData = await DomDetailerAssistant.GetDetailsAsync(domain.Address, DefaultDomDetailerApiKey); //use the default key from here

            if (domDetailerData == null)
            {
                domain.Stats = "Couldn't get the stats";
            }
            else if (domDetailerData.WrongCredentials)
            {
                domain.Stats = "The api key seems to be wrong, please double check";
            }
            else
            {
                domain.StatsLoaded = true;
                domain.Stats = domDetailerData.ToString();
                domain.StatsData = domDetailerData;
            }

            domain.IsGettingStats = false;
        }

        private void GetDomDetailerTrialDialogResult()
        {
            if (_dialogShown)
                return;

            lock (_lock)
            {
                if (_dialogShown)
                    return;

                _dialogShown = true;

                new DomDetailerTrialDialog().ShowDialog();
            }
        }
    }
}

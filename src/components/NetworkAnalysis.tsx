import React, { useState, useEffect, useRef } from 'react';
import { Activity, Globe, RefreshCcw, Check } from 'lucide-react';
import { LatencyMonitorWidget } from './LatencyMonitorWidget';
import { PingTestWidget } from "./PingTestWidget";
import { useLanguage } from '../contexts/LanguageContext';
import { ConfirmationModal } from './ConfirmationModal';
import { motion, AnimatePresence } from 'motion/react';
import { cn } from '../lib/utils';

export function NetworkAnalysis() {
  const { t } = useLanguage();
  const [ping, setPing] = useState<number | null>(null);
  const [dns, setDns] = useState('auto');
  const [applyingDns, setApplyingDns] = useState(false);
  const [dnsSuccess, setDnsSuccess] = useState(false);
  const [modalOpen, setModalOpen] = useState(false);
  const [resetting, setResetting] = useState(false);
  const [resetSuccess, setResetSuccess] = useState(false);

  const timeoutsRef = useRef<NodeJS.Timeout[]>([]);

  useEffect(() => {
    const interval = setInterval(() => {
       setPing(p => Math.max(8, Math.min(120, p + (Math.random() * 10 - 5))));
    }, 1500);
    
    return () => {
      clearInterval(interval);
      timeoutsRef.current.forEach(clearTimeout);
    };
  }, []);

  const handleApplyDns = () => {
    setApplyingDns(true);
    const t1 = setTimeout(() => {
      setApplyingDns(false);
      setDnsSuccess(true);
      const t2 = setTimeout(() => setDnsSuccess(false), 3000);
      timeoutsRef.current.push(t2);
    }, 1500);
    timeoutsRef.current.push(t1);
  };

  const confirmReset = () => {
    setModalOpen(false);
    setResetting(true);
    const t1 = setTimeout(() => {
      setResetting(false);
      setResetSuccess(true);
      const t2 = setTimeout(() => setResetSuccess(false), 4000);
      timeoutsRef.current.push(t2);
    }, 2500);
    timeoutsRef.current.push(t1);
  };

  return (
    <div className="space-y-8 animate-in fade-in slide-in-from-bottom-4 duration-500 flex flex-col h-full">
      <div>
        <h2 className="text-2xl font-semibold text-zinc-900 dark:text-zinc-50">{t('networkTab')}</h2>
        <p className="text-zinc-500 dark:text-zinc-400 mt-1">{t('networkDesc')}</p>
      </div>
      <div className="grid grid-cols-3 gap-6">
        
        {/* Card 1: Ping */}
        <div className="bg-white/50 dark:bg-zinc-900/50 backdrop-blur-sm border border-zinc-200 dark:border-zinc-800 rounded-2xl p-5 shadow-sm">
          <div className="flex items-center gap-3 mb-4">
            <div className="p-2 bg-blue-100 dark:bg-blue-500/20 text-blue-600 dark:text-blue-500 rounded-lg">
              <Activity className="w-5 h-5" />
            </div>
            <h4 className="font-medium text-zinc-900 dark:text-zinc-100 text-sm">{t('pingLabel')}</h4>
          </div>
          <div className="flex items-end gap-2">
            <span className="text-3xl font-bold text-zinc-900 dark:text-zinc-50">{ping !== null ? Math.round(ping) : '--'}</span>
            <span className="text-zinc-500 font-medium mb-1">ms</span>
          </div>
          {ping !== null ? (
            <p className="text-xs text-emerald-500 mt-2 flex items-center gap-1.5 font-medium">
              <span className="w-2 h-2 rounded-full bg-emerald-500 animate-pulse" />
              {t('statusOnline')}
            </p>
          ) : (
            <p className="text-xs text-amber-500 mt-2 flex items-center gap-1.5 font-medium">
              <span className="w-2 h-2 rounded-full bg-amber-500 animate-pulse" />
              {t('waitingData')}
            </p>
          )}
        </div>

        {/* Card 2: DNS */}
        <div className="bg-white/50 dark:bg-zinc-900/50 backdrop-blur-sm border border-zinc-200 dark:border-zinc-800 rounded-2xl p-5 shadow-sm flex flex-col justify-between">
          <div>
            <div className="flex items-center gap-3 mb-4">
              <div className="p-2 bg-purple-100 dark:bg-purple-500/20 text-purple-600 dark:text-purple-500 rounded-lg">
                <Globe className="w-5 h-5" />
              </div>
              <h4 className="font-medium text-zinc-900 dark:text-zinc-100 text-sm">{t('dnsLabel')}</h4>
            </div>
            <div className="mb-4">
              <select
                value={dns}
                onChange={(e) => setDns(e.target.value)}
                className="w-full bg-zinc-100 dark:bg-zinc-800 border-none rounded-xl px-3 py-2.5 text-sm text-zinc-900 dark:text-zinc-100 outline-none focus:ring-2 focus:ring-purple-500/50 transition-shadow appearance-none"
              >
                <option value="auto">{t('dnsAuto')}</option>
                <option value="google">Google (8.8.8.8)</option>
                <option value="cloudflare">Cloudflare (1.1.1.1)</option>
                <option value="opendns">OpenDNS (208.67.222.222)</option>
              </select>
            </div>
          </div>
          <button
            onClick={handleApplyDns}
            disabled={applyingDns || dnsSuccess}
            className="w-full bg-zinc-900 dark:bg-zinc-100 hover:bg-zinc-800 dark:hover:bg-zinc-200 text-white dark:text-zinc-900 py-2.5 rounded-xl text-sm font-medium disabled:opacity-50 transition-colors flex justify-center items-center h-10"
          >
            {applyingDns ? (
              <motion.div initial={{ opacity: 0 }} animate={{ opacity: 1 }} className="flex items-center gap-2">
                <div className="w-4 h-4 border-2 border-current border-t-transparent rounded-full animate-spin" />
              </motion.div>
            ) : dnsSuccess ? (
              <Check className="w-5 h-5 text-emerald-500 dark:text-emerald-600" />
            ) : (
              t('apply')
            )}
          </button>
        </div>

        {/* Card 3: Reset */}
        <div className="bg-white/50 dark:bg-zinc-900/50 backdrop-blur-sm border border-zinc-200 dark:border-zinc-800 rounded-2xl p-5 shadow-sm flex flex-col justify-between">
          <div>
            <div className="flex items-center gap-3 mb-2">
              <div className="p-2 bg-rose-100 dark:bg-rose-500/20 text-rose-600 dark:text-rose-500 rounded-lg">
                <RefreshCcw className="w-5 h-5" />
              </div>
              <h4 className="font-medium text-zinc-900 dark:text-zinc-100 text-sm">{t('netResetTitle')}</h4>
            </div>
            <p className="text-xs text-zinc-500 dark:text-zinc-400 leading-relaxed mb-4">
              {t('netResetDesc')}
            </p>
          </div>
          <button
            onClick={() => setModalOpen(true)}
            disabled={resetting || resetSuccess}
            className="w-full bg-rose-100 dark:bg-rose-500/10 hover:bg-rose-200 dark:hover:bg-rose-500/20 text-rose-600 dark:text-rose-400 py-2.5 rounded-xl text-sm font-medium transition-colors flex justify-center items-center h-10"
          >
            {resetting ? (
              <motion.div initial={{ opacity: 0 }} animate={{ opacity: 1 }} className="flex items-center gap-2">
                <div className="w-4 h-4 border-2 border-current border-t-transparent rounded-full animate-spin" />
              </motion.div>
            ) : resetSuccess ? (
              <Check className="w-5 h-5" />
            ) : (
              t('runReset')
            )}
          </button>
        </div>

      </div>

      <PingTestWidget />
      <LatencyMonitorWidget />

      <ConfirmationModal
        isOpen={modalOpen}
        title={t('modalNetTitle')}
        description={t('modalNetDesc')}
        onConfirm={confirmReset}
        onCancel={() => setModalOpen(false)}
        confirmText={t('runReset')}
        cancelText={t('cancel')}
        isDestructive={true}
      />
    </div>
  );
}

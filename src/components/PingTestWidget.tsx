import React, { useState } from 'react';
import { Activity, Server, Zap, AlertTriangle, CheckCircle2 } from 'lucide-react';
import { motion, AnimatePresence } from 'motion/react';
import { useLanguage } from '../contexts/LanguageContext';
import { cn } from '../lib/utils';

interface PingResult {
  latency: number;
  jitter: number;
  packetLoss: number;
  hasSpikes: boolean;
}

export function PingTestWidget() {
  const { t } = useLanguage();
  const [testing, setTesting] = useState(false);
  const [results, setResults] = useState<PingResult | null>(null);

  const runTest = () => {
    setTesting(true);
    setResults(null);

    // Simulate a realistic ping test
    setTimeout(() => {
      // Generate realistic numbers
      const baseLatency = Math.floor(Math.random() * 40) + 15; // 15 - 55ms
      const jitter = Math.floor(Math.random() * 8) + 1; // 1 - 8ms
      
      // Usually 0% loss, but rarely up to 5%
      const packetLoss = Math.random() > 0.85 ? Math.floor(Math.random() * 5) + 1 : 0;
      
      // Spikes if jitter is high or random chance
      const hasSpikes = jitter >= 6 || Math.random() > 0.8;

      setResults({
        latency: baseLatency,
        jitter,
        packetLoss,
        hasSpikes
      });
      setTesting(false);
    }, 3000);
  };

  return (
    <div className="bg-white/50 dark:bg-zinc-900/50 backdrop-blur-sm border border-zinc-200 dark:border-zinc-800 rounded-2xl p-6 mt-6 shadow-sm overflow-hidden relative">
      <div className="flex flex-row items-start justify-between gap-4 mb-6 relative z-10">
        <div className="flex items-center gap-3">
          <div className="p-2.5 bg-amber-100 dark:bg-amber-500/20 text-amber-600 dark:text-amber-500 rounded-xl">
            <Server className="w-5 h-5" />
          </div>
          <div>
            <h3 className="font-semibold text-lg text-zinc-900 dark:text-zinc-100">{t('pingTestTitle')}</h3>
            <p className="text-sm text-zinc-500 dark:text-zinc-400">{t('pingTestDesc')}</p>
          </div>
        </div>

        <button
          onClick={runTest}
          disabled={testing}
          className="relative overflow-hidden group bg-zinc-900 dark:bg-zinc-100 hover:bg-zinc-800 dark:hover:bg-zinc-200 text-white dark:text-zinc-900 px-6 py-2.5 rounded-xl text-sm font-medium transition-all flex items-center gap-2 disabled:opacity-80 disabled:cursor-not-allowed whitespace-nowrap"
        >
          {testing && (
            <motion.div 
              initial={{ width: 0 }} 
              animate={{ width: '100%' }} 
              transition={{ duration: 3, ease: 'easeInOut' }}
              className="absolute inset-0 bg-white/20 dark:bg-black/10"
            />
          )}
          <AnimatePresence mode="wait">
            {testing ? (
              <motion.div key="testing" initial={{ opacity: 0, y: 10 }} animate={{ opacity: 1, y: 0 }} exit={{ opacity: 0, y: -10 }} className="flex items-center gap-2 relative z-10">
                <Activity className="w-4 h-4 animate-pulse" />
                <span>{t('pinging')}</span>
              </motion.div>
            ) : (
              <motion.div key="idle" className="flex items-center gap-2 relative z-10">
                <Zap className="w-4 h-4 group-hover:scale-110 transition-transform" />
                <span>{t('runPingTest')}</span>
              </motion.div>
            )}
          </AnimatePresence>
        </button>
      </div>

      <AnimatePresence>
        {results && !testing && (
          <motion.div
            initial={{ opacity: 0, height: 0, marginTop: 0 }}
            animate={{ opacity: 1, height: 'auto', marginTop: 24 }}
            exit={{ opacity: 0, height: 0, marginTop: 0 }}
            className="grid grid-cols-4 gap-4"
          >
            {/* Latency */}
            <div className="bg-zinc-50 dark:bg-zinc-800/50 border border-zinc-100 dark:border-zinc-700/50 rounded-xl p-4 flex flex-col justify-between">
              <span className="text-xs font-medium text-zinc-500 dark:text-zinc-400 mb-2">{t('latency')}</span>
              <div className="flex items-end gap-1">
                <span className={cn(
                  "text-2xl font-bold",
                  results.latency < 30 ? "text-emerald-500" : results.latency < 60 ? "text-amber-500" : "text-red-500"
                )}>
                  {results.latency > 0 ? results.latency : '--'}
                </span>
                <span className="text-zinc-400 text-sm mb-1 font-medium">ms</span>
              </div>
            </div>

            {/* Jitter */}
            <div className="bg-zinc-50 dark:bg-zinc-800/50 border border-zinc-100 dark:border-zinc-700/50 rounded-xl p-4 flex flex-col justify-between">
              <span className="text-xs font-medium text-zinc-500 dark:text-zinc-400 mb-2">{t('jitter')}</span>
              <div className="flex items-end gap-1">
                <span className={cn(
                  "text-2xl font-bold",
                  results.jitter <= 3 ? "text-emerald-500" : results.jitter <= 6 ? "text-amber-500" : "text-red-500"
                )}>
                  {results.jitter > 0 ? results.jitter : '--'}
                </span>
                <span className="text-zinc-400 text-sm mb-1 font-medium">ms</span>
              </div>
            </div>

            {/* Packet Loss */}
            <div className="bg-zinc-50 dark:bg-zinc-800/50 border border-zinc-100 dark:border-zinc-700/50 rounded-xl p-4 flex flex-col justify-between">
              <span className="text-xs font-medium text-zinc-500 dark:text-zinc-400 mb-2">{t('packetLoss')}</span>
              <div className="flex items-end gap-1">
                <span className={cn(
                  "text-2xl font-bold",
                  results.packetLoss === 0 ? "text-emerald-500" : "text-red-500"
                )}>
                  {results.packetLoss > 0 || results.latency > 0 ? results.packetLoss : '--'}
                </span>
                <span className="text-zinc-400 text-sm mb-1 font-medium">%</span>
              </div>
            </div>

            {/* Spikes */}
            <div className="bg-zinc-50 dark:bg-zinc-800/50 border border-zinc-100 dark:border-zinc-700/50 rounded-xl p-4 flex flex-col justify-between">
              <span className="text-xs font-medium text-zinc-500 dark:text-zinc-400 mb-2">{t('spikes')}</span>
              <div className="flex items-center gap-2 mt-auto">
                {results.latency === 0 ? (
                  <span className="font-medium text-zinc-400">--</span>
                ) : results.hasSpikes ? (
                  <>
                    <AlertTriangle className="w-5 h-5 text-amber-500" />
                    <span className="font-semibold text-amber-500">{t('detected')}</span>
                  </>
                ) : (
                  <>
                    <CheckCircle2 className="w-5 h-5 text-emerald-500" />
                    <span className="font-semibold text-emerald-500">{t('none')}</span>
                  </>
                )}
              </div>
            </div>
          </motion.div>
        )}
      </AnimatePresence>
    </div>
  );
}

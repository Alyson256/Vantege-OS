import React from 'react';
import { Server, Loader2 } from 'lucide-react';
import { HardwareMonitor } from './HardwareMonitor';
import { OptimizationScore } from './OptimizationScore';
import { JunkFilesWidget } from './JunkFilesWidget';
import { RamOptimizerWidget } from './RamOptimizerWidget';
import { useLanguage } from '../contexts/LanguageContext';

export function Dashboard() {
  const { t } = useLanguage();

  return (
    <div className="space-y-8 animate-in fade-in slide-in-from-bottom-4 duration-500 relative">
      <div className="flex flex-row items-center justify-between gap-4">
        <div>
          <h2 className="text-2xl font-semibold text-zinc-900 dark:text-zinc-50">{t('dashboard')}</h2>
          <p className="text-zinc-500 dark:text-zinc-400 mt-1">{t('dashDesc')}</p>
        </div>
      </div>

      {/* Backend Status Banner */}
      <div className="bg-amber-50/80 dark:bg-amber-500/10 border border-amber-200/80 dark:border-amber-500/20 rounded-2xl p-4 flex items-center gap-4 backdrop-blur-sm">
        <div className="relative flex items-center justify-center">
          <Server className="w-6 h-6 text-amber-500 dark:text-amber-400" />
          <Loader2 className="w-8 h-8 text-amber-500/50 dark:text-amber-400/50 animate-spin absolute" />
        </div>
        <div>
          <h3 className="text-sm font-semibold text-amber-800 dark:text-amber-300">
            {t('waitingBackendTitle')}
          </h3>
          <p className="text-xs text-amber-700 dark:text-amber-400/80 mt-0.5">
            {t('waitingBackendDesc')}
          </p>
        </div>
      </div>

      <OptimizationScore />
      
      <HardwareMonitor />
      
      <div className="grid grid-cols-2 gap-8">
        <RamOptimizerWidget />
        <JunkFilesWidget />
      </div>
    </div>
  );
}

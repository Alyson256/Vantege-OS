import React, { useEffect, useState } from 'react';
import { cn } from '../lib/utils';
import { Activity, Info } from 'lucide-react';
import { Tooltip } from './Tooltip';
import { useLanguage } from '../contexts/LanguageContext';

export function OptimizationScore() {
  const [score, setScore] = useState<number | null>(null);
  const [dpc, setDpc] = useState<number | null>(null);
  const [isr, setIsr] = useState<number | null>(null);
  const [pageFaults, setPageFaults] = useState<number | null>(null);
  const { t } = useLanguage();

  // Simulação de telemetria em tempo real
  useEffect(() => {
    // Awaiting backend connection
  }, []);

  // Cálculos do Gauge (Círculo SVG)
  const radius = 80;
  const circumference = Math.PI * radius;
  // Limitar score de 0 a 100 para o cálculo do offset
  const normalizedScore = score !== null ? Math.min(100, Math.max(0, score)) : 0;
  const dashoffset = circumference - (normalizedScore / 100) * circumference;

  const getColor = (s: number | null) => {
    if (s === null) return 'text-zinc-300 dark:text-zinc-700';
    if (s >= 85) return 'text-emerald-500';
    if (s >= 60) return 'text-amber-500';
    return 'text-rose-500';
  };

  const getStatusText = (s: number | null) => {
    if (s === null) return t('waitingBackendTitle') as string;
    
    if (s >= 85) return t('latOpt');
    if (s >= 60) return t('latWarn');
    return t('latDanger');
  };

  return (
    <div className="bg-white/50 dark:bg-zinc-900/50 backdrop-blur-sm border border-zinc-200 dark:border-zinc-800 rounded-2xl p-8 shadow-sm flex flex-row items-center gap-8 lg:gap-12">
      
      {/* Gauge Visualizer */}
      <div className="flex flex-col items-center flex-shrink-0">
        <div className="relative w-56 h-32 flex items-end justify-center">
          <svg className="absolute top-0 left-0 w-full h-full drop-shadow-sm" viewBox="0 0 200 120">
            {/* Background Arc */}
            <path
              d="M 20 100 A 80 80 0 0 1 180 100"
              fill="none"
              stroke="currentColor"
              strokeWidth="14"
              strokeLinecap="round"
              className="text-zinc-200 dark:text-zinc-800"
            />
            {/* Foreground Arc */}
            <path
              d="M 20 100 A 80 80 0 0 1 180 100"
              fill="none"
              stroke="currentColor"
              strokeWidth="14"
              strokeLinecap="round"
              className={getColor(score)}
              strokeDasharray={circumference}
              strokeDashoffset={dashoffset}
              style={{ transition: 'stroke-dashoffset 1s ease-in-out, stroke 1s ease-in-out' }}
            />
          </svg>
          <div className="text-center pb-2">
            <span className="text-5xl font-bold tracking-tight text-zinc-900 dark:text-zinc-50">
              {score !== null ? Math.round(score) : '--'}
            </span>
            <span className="text-xs font-bold text-zinc-500 dark:text-zinc-400 block uppercase tracking-widest mt-1">
              Score
            </span>
          </div>
        </div>
      </div>

      {/* Metrics & Information */}
      <div className="flex-1 w-full space-y-6">
        <div>
          <h3 className="text-lg font-semibold text-zinc-900 dark:text-zinc-100 flex items-center gap-2">
            <Activity className={cn("w-5 h-5", getColor(score))} />
            {t('latAnalysis')}
          </h3>
          <p className="text-sm text-zinc-500 dark:text-zinc-400 mt-2 leading-relaxed max-w-2xl">
            {getStatusText(score)}
          </p>
        </div>

        <div className="grid grid-cols-2 gap-x-6 gap-y-4 pt-5 border-t border-zinc-200 dark:border-zinc-800">
          <div>
            <Tooltip content={t('tooltipDpc') as string} position="top" className="mb-1 w-max">
              <div className="flex items-center gap-1.5 cursor-help">
                <p className="text-xs font-medium text-zinc-500 dark:text-zinc-500">Highest DPC Routine Execution Time</p>
                <Info className="w-3.5 h-3.5 text-zinc-400 dark:text-zinc-500" />
              </div>
            </Tooltip>
            <p className="font-mono text-sm font-medium text-zinc-900 dark:text-zinc-100">{dpc !== null ? Math.round(dpc) + ' μs' : '--'}</p>
          </div>
          
          <div>
            <Tooltip content={t('tooltipIsr') as string} position="top" className="mb-1 w-max">
              <div className="flex items-center gap-1.5 cursor-help">
                <p className="text-xs font-medium text-zinc-500 dark:text-zinc-500">Highest ISR Routine Execution Time</p>
                <Info className="w-3.5 h-3.5 text-zinc-400 dark:text-zinc-500" />
              </div>
            </Tooltip>
            <p className="font-mono text-sm font-medium text-zinc-900 dark:text-zinc-100">{isr !== null ? Math.round(isr) + ' μs' : '--'}</p>
          </div>
          
          <div>
            <Tooltip content={t('tooltipPagefaults') as string} position="top" className="mb-1 w-max">
              <div className="flex items-center gap-1.5 cursor-help">
                <p className="text-xs font-medium text-zinc-500 dark:text-zinc-500">Hard Pagefaults Count</p>
                <Info className="w-3.5 h-3.5 text-zinc-400 dark:text-zinc-500" />
              </div>
            </Tooltip>
            <p className="font-mono text-sm font-medium text-emerald-500 dark:text-emerald-400">{pageFaults !== null ? Math.max(0, pageFaults).toFixed(1) : '--'}</p>
          </div>
          
          <div>
            <Tooltip content={t('tooltipReliability') as string} position="top" className="mb-1 w-max">
              <div className="flex items-center gap-1.5 cursor-help">
                <p className="text-xs font-medium text-zinc-500 dark:text-zinc-500">{t('reliability')}</p>
                <Info className="w-3.5 h-3.5 text-zinc-400 dark:text-zinc-500" />
              </div>
            </Tooltip>
            <p className="font-mono text-sm font-medium text-emerald-500 dark:text-emerald-400">{score !== null ? t('excellent') : '--'}</p>
          </div>
        </div>
      </div>

    </div>
  );
}

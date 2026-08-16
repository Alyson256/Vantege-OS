import React, { useState, useEffect, useRef } from 'react';
import { MemoryStick, Zap, CheckCircle2 } from 'lucide-react';
import { motion, AnimatePresence } from 'motion/react';
import { useLanguage } from '../contexts/LanguageContext';
import { useActionHistory } from '../contexts/ActionHistoryContext';

export function RamOptimizerWidget() {
  const { t } = useLanguage();
  const { registerAction } = useActionHistory();
  const [optimizing, setOptimizing] = useState(false);
  const [optimized, setOptimized] = useState(false);
  
  // States in GB
  const totalRam = 0;
  const [usedRam, setUsedRam] = useState(0); 

  const availableRam = totalRam - usedRam;
  const usedPercentage = totalRam > 0 ? (usedRam / totalRam) * 100 : 0;
  const availablePercentage = totalRam > 0 ? (availableRam / totalRam) * 100 : 0;

  const timeoutsRef = useRef<NodeJS.Timeout[]>([]);

  useEffect(() => {
    return () => {
      timeoutsRef.current.forEach(clearTimeout);
    };
  }, []);

  const handleOptimize = () => {
    if (optimizing || optimized || usedRam < 6) return;
    setOptimizing(true);
    
    const t1 = setTimeout(() => {
      setOptimizing(false);
      // Simula a liberação de uns 4.5GB de RAM em cache
      setUsedRam(prev => Math.max(4.2, prev - 4.5));
      setOptimized(true);
      
      const t2 = setTimeout(() => setOptimized(false), 3000);
      timeoutsRef.current.push(t2);
    }, 2000);
    
    timeoutsRef.current.push(t1);
  };

  return (
    <div className="bg-white/50 dark:bg-zinc-900/50 backdrop-blur-sm border border-zinc-200 dark:border-zinc-800 rounded-2xl p-6">
      <div className="flex flex-row justify-between items-center gap-4 mb-6">
        <div>
          <h3 className="text-lg font-semibold text-zinc-900 dark:text-zinc-100 flex items-center gap-2">
            <MemoryStick className="w-5 h-5 text-zinc-500" />
            {t('ramTitle')}
          </h3>
          <p className="text-sm text-zinc-500 dark:text-zinc-400 mt-1">
            {t('ramDesc')}
          </p>
        </div>
        
        <button
          onClick={handleOptimize}
          disabled={optimizing || optimized || usedRam < 6}
          className="w-auto relative overflow-hidden bg-blue-600 dark:bg-blue-500 text-white px-4 py-2 rounded-xl font-medium shadow-sm transition-all flex items-center justify-center gap-2 disabled:opacity-50 disabled:cursor-not-allowed hover:bg-blue-700 dark:hover:bg-blue-600"
        >
          {optimizing && (
            <motion.div 
              initial={{ width: 0 }} 
              animate={{ width: '100%' }} 
              transition={{ duration: 2, ease: 'easeInOut' }}
              className="absolute inset-0 bg-white/20 dark:bg-black/10"
            />
          )}
          <AnimatePresence mode="wait">
            {optimizing ? (
              <motion.div key="optimizing" initial={{ opacity: 0 }} animate={{ opacity: 1 }} exit={{ opacity: 0 }} className="flex items-center gap-2 relative z-10">
                <div className="w-4 h-4 border-2 border-current border-t-transparent rounded-full animate-spin" />
                <span>{t('optimizingRam')}</span>
              </motion.div>
            ) : optimized ? (
              <motion.div key="optimized" initial={{ opacity: 0, scale: 0.8 }} animate={{ opacity: 1, scale: 1 }} className="flex items-center gap-2 relative z-10">
                <CheckCircle2 className="w-4 h-4" />
                <span>{t('optimized')}</span>
              </motion.div>
            ) : (
              <motion.div key="idle" className="flex items-center gap-2 relative z-10">
                <Zap className="w-4 h-4" />
                <span>{t('optimize')}</span>
              </motion.div>
            )}
          </AnimatePresence>
        </button>
      </div>

      <div className="space-y-5">
        {/* Used RAM */}
        <div>
          <div className="flex justify-between items-center text-sm mb-1.5">
            <span className="font-medium text-blue-600 dark:text-blue-500">{t('ramUsed')}</span>
            <span className="font-mono font-medium text-zinc-900 dark:text-zinc-100">
              {totalRam > 0 ? usedRam.toFixed(1) + ' GB' : '-- GB'}
            </span>
          </div>
          <div className="h-3 w-full bg-zinc-100 dark:bg-zinc-800 rounded-full overflow-hidden">
            <motion.div 
              initial={{ width: 0 }}
              animate={{ width: `${usedPercentage}%` }}
              transition={{ duration: 1, ease: 'easeOut' }}
              className="h-full bg-blue-500 rounded-full"
            />
          </div>
        </div>

        {/* Free RAM */}
        <div>
          <div className="flex justify-between items-center text-sm mb-1.5">
            <span className="font-medium text-zinc-500 dark:text-zinc-400">{t('ramAvailable')}</span>
            <span className="font-mono font-medium text-zinc-900 dark:text-zinc-100">
              {totalRam > 0 ? availableRam.toFixed(1) + ' GB' : '-- GB'}
            </span>
          </div>
          <div className="h-3 w-full bg-zinc-100 dark:bg-zinc-800 rounded-full overflow-hidden">
            <motion.div 
              initial={{ width: 0 }}
              animate={{ width: `${availablePercentage}%` }}
              transition={{ duration: 1, ease: 'easeOut' }}
              className="h-full bg-zinc-300 dark:bg-zinc-600 rounded-full"
            />
          </div>
        </div>
      </div>
    </div>
  );
}

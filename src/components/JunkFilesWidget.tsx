import React, { useState, useEffect, useRef } from 'react';
import { Trash2, HardDrive, CheckCircle2 } from 'lucide-react';
import { motion, AnimatePresence } from 'motion/react';
import { useLanguage } from '../contexts/LanguageContext';
import { useActionHistory } from '../contexts/ActionHistoryContext';

export function JunkFilesWidget() {
  const { t } = useLanguage();
  const { registerAction } = useActionHistory();
  const [cleaning, setCleaning] = useState(false);
  const [cleaned, setCleaned] = useState(false);
  const [tempSpace, setTempSpace] = useState(0); // GB

  const totalSpace = 0;
  const freeSpace = 0; 
  const tempPercentage = totalSpace > 0 ? (tempSpace / totalSpace) * 100 : 0;
  const freePercentage = totalSpace > 0 ? (freeSpace / totalSpace) * 100 : 0;

  const timeoutsRef = useRef<NodeJS.Timeout[]>([]);

  useEffect(() => {
    return () => {
      timeoutsRef.current.forEach(clearTimeout);
    };
  }, []);

  const handleClean = () => {
    if (cleaning || cleaned || tempSpace === 0) return;
    setCleaning(true);
    
    const t1 = setTimeout(() => {
      setCleaning(false);
      setTempSpace(0);
      setCleaned(true);
      registerAction({
        type: 'junk_clean',
        title: t('junkFilesTitle')
      });
      
      const t2 = setTimeout(() => setCleaned(false), 3000);
      timeoutsRef.current.push(t2);
    }, 2000);
    
    timeoutsRef.current.push(t1);
  };

  return (
    <div className="bg-white/50 dark:bg-zinc-900/50 backdrop-blur-sm border border-zinc-200 dark:border-zinc-800 rounded-2xl p-6">
      <div className="flex flex-row justify-between items-center gap-4 mb-6">
        <div>
          <h3 className="text-lg font-semibold text-zinc-900 dark:text-zinc-100 flex items-center gap-2">
            <HardDrive className="w-5 h-5 text-zinc-500" />
            {t('junkFilesTitle')}
          </h3>
          <p className="text-sm text-zinc-500 dark:text-zinc-400 mt-1">
            {t('junkFilesDesc')}
          </p>
        </div>
        
        <button
          onClick={handleClean}
          disabled={cleaning || cleaned || tempSpace === 0}
          className="w-auto relative overflow-hidden bg-zinc-900 dark:bg-zinc-100 text-white dark:text-zinc-900 px-4 py-2 rounded-xl font-medium shadow-sm transition-all flex items-center justify-center gap-2 disabled:opacity-50 disabled:cursor-not-allowed hover:bg-zinc-800 dark:hover:bg-zinc-200"
        >
          {cleaning && (
            <motion.div 
              initial={{ width: 0 }} 
              animate={{ width: '100%' }} 
              transition={{ duration: 2, ease: 'easeInOut' }}
              className="absolute inset-0 bg-white/20 dark:bg-black/10"
            />
          )}
          <AnimatePresence mode="wait">
            {cleaning ? (
              <motion.div key="cleaning" initial={{ opacity: 0 }} animate={{ opacity: 1 }} exit={{ opacity: 0 }} className="flex items-center gap-2 relative z-10">
                <div className="w-4 h-4 border-2 border-current border-t-transparent rounded-full animate-spin" />
                <span>{t('cleaningJunk')}</span>
              </motion.div>
            ) : cleaned ? (
              <motion.div key="cleaned" initial={{ opacity: 0, scale: 0.8 }} animate={{ opacity: 1, scale: 1 }} className="flex items-center gap-2 relative z-10 text-emerald-400 dark:text-emerald-600">
                <CheckCircle2 className="w-4 h-4" />
                <span>{t('junkCleaned')}</span>
              </motion.div>
            ) : (
              <motion.div key="idle" className="flex items-center gap-2 relative z-10">
                <Trash2 className="w-4 h-4" />
                <span>{t('cleanJunk')}</span>
              </motion.div>
            )}
          </AnimatePresence>
        </button>
      </div>

      <div className="space-y-5">
        {/* Temporary Files Bar */}
        <div>
          <div className="flex justify-between items-center text-sm mb-1.5">
            <span className="font-medium text-amber-600 dark:text-amber-500">{t('tempFiles')}</span>
            <span className="font-mono font-medium text-zinc-900 dark:text-zinc-100">
              {totalSpace > 0 ? (tempSpace > 0 ? tempSpace.toFixed(1) + ' GB' : '0 MB') : '--'}
            </span>
          </div>
          <div className="h-3 w-full bg-zinc-100 dark:bg-zinc-800 rounded-full overflow-hidden">
            <motion.div 
              initial={{ width: 0 }}
              animate={{ width: `${Math.max(tempPercentage, tempSpace > 0 ? 2 : 0)}%` }}
              transition={{ duration: 1, ease: 'easeOut' }}
              className="h-full bg-amber-500 rounded-full"
            />
          </div>
        </div>

        {/* Free Space Bar */}
        <div>
          <div className="flex justify-between items-center text-sm mb-1.5">
            <span className="font-medium text-emerald-600 dark:text-emerald-500">{t('freeSpace')}</span>
            <span className="font-mono font-medium text-zinc-900 dark:text-zinc-100">
              {totalSpace > 0 ? freeSpace.toFixed(1) + ' GB' : '-- GB'}
            </span>
          </div>
          <div className="h-3 w-full bg-zinc-100 dark:bg-zinc-800 rounded-full overflow-hidden">
            <motion.div 
              initial={{ width: 0 }}
              animate={{ width: `${freePercentage}%` }}
              transition={{ duration: 1, ease: 'easeOut' }}
              className="h-full bg-emerald-500 rounded-full"
            />
          </div>
        </div>
      </div>
    </div>
  );
}

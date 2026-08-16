import React, { useState, useEffect, useRef } from 'react';
import { RefreshCw, Download, CheckCircle2 } from 'lucide-react';
import { motion, AnimatePresence } from 'motion/react';
import { useLanguage } from '../contexts/LanguageContext';

export function UpdateButton() {
  const { t } = useLanguage();
  const [updateState, setUpdateState] = useState<'idle' | 'checking' | 'installing' | 'updated'>('idle');
  const timeoutsRef = useRef<NodeJS.Timeout[]>([]);

  useEffect(() => {
    return () => {
      timeoutsRef.current.forEach(clearTimeout);
    };
  }, []);

  const handleCheckUpdate = () => {
    if (updateState !== 'idle') return;
    setUpdateState('checking');
    
    const t1 = setTimeout(() => {
      setUpdateState('installing');
      const t2 = setTimeout(() => {
        setUpdateState('updated');
        const t3 = setTimeout(() => setUpdateState('idle'), 3000);
        timeoutsRef.current.push(t3);
      }, 2500);
      timeoutsRef.current.push(t2);
    }, 2000);
    
    timeoutsRef.current.push(t1);
  };

  return (
    <button 
      onClick={handleCheckUpdate}
      disabled={updateState !== 'idle'}
      className="w-full flex items-center gap-3 px-4 py-3 rounded-xl font-medium text-sm text-zinc-600 dark:text-zinc-400 bg-zinc-100/50 dark:bg-zinc-800/30 hover:bg-zinc-100 dark:hover:bg-zinc-800/80 transition-colors disabled:opacity-70 disabled:cursor-wait relative overflow-hidden"
    >
      <AnimatePresence mode="wait">
        {updateState === 'checking' && (
          <motion.div key="checking" initial={{ opacity: 0 }} animate={{ opacity: 1 }} exit={{ opacity: 0 }} className="flex items-center gap-3 text-blue-600 dark:text-blue-400">
            <RefreshCw className="w-5 h-5 animate-spin" />
            <span>{t('checkingUpdates')}</span>
          </motion.div>
        )}
        {updateState === 'installing' && (
          <motion.div key="installing" initial={{ opacity: 0 }} animate={{ opacity: 1 }} exit={{ opacity: 0 }} className="flex items-center gap-3 text-amber-600 dark:text-amber-400">
            <Download className="w-5 h-5 animate-bounce" />
            <span>{t('installingUpdate')}</span>
          </motion.div>
        )}
        {updateState === 'updated' && (
          <motion.div key="updated" initial={{ opacity: 0, scale: 0.8 }} animate={{ opacity: 1, scale: 1 }} exit={{ opacity: 0 }} className="flex items-center gap-3 text-emerald-600 dark:text-emerald-400">
            <CheckCircle2 className="w-5 h-5" />
            <span>{t('updated')}</span>
          </motion.div>
        )}
        {updateState === 'idle' && (
          <motion.div key="idle" initial={{ opacity: 0 }} animate={{ opacity: 1 }} exit={{ opacity: 0 }} className="flex items-center gap-3">
            <RefreshCw className="w-5 h-5" />
            <span>{t('checkUpdates')}</span>
          </motion.div>
        )}
      </AnimatePresence>
    </button>
  );
}

import React, { useEffect } from 'react';
import { motion, AnimatePresence } from 'motion/react';
import { RotateCcw, X } from 'lucide-react';
import { useActionHistory } from '../contexts/ActionHistoryContext';
import { useLanguage } from '../contexts/LanguageContext';

export function GlobalUndoWidget() {
  const { lastAction, undoLastAction, clearAction, isUndoing } = useActionHistory();
  const { t } = useLanguage();

  useEffect(() => {
    if (lastAction && !isUndoing) {
      const timer = setTimeout(() => {
        clearAction();
      }, 10000);
      return () => clearTimeout(timer);
    }
  }, [lastAction, isUndoing, clearAction]);

  return (
    <AnimatePresence>
      {lastAction && (
        <motion.div
          initial={{ opacity: 0, y: 50, scale: 0.9 }}
          animate={{ opacity: 1, y: 0, scale: 1 }}
          exit={{ opacity: 0, scale: 0.9, y: 50 }}
          className="fixed bottom-6 left-1/2 -translate-x-1/2 z-[90] flex flex-col bg-zinc-900 dark:bg-zinc-800 text-white dark:text-zinc-100 rounded-2xl shadow-2xl shadow-black/20 border border-zinc-800 dark:border-zinc-700 overflow-hidden"
        >
          <div className="flex items-center gap-4 px-4 py-3">
            <div className="flex flex-col">
              <span className="text-sm font-medium">{t('undoLastAction')}</span>
              <span className="text-xs text-zinc-400 max-w-[200px] truncate">{lastAction.title}</span>
            </div>
            
            <div className="flex items-center gap-2">
              <button
                onClick={undoLastAction}
                disabled={isUndoing}
                className="flex items-center gap-2 px-3 py-1.5 rounded-lg bg-zinc-800 dark:bg-zinc-700 hover:bg-zinc-700 dark:hover:bg-zinc-600 transition-colors disabled:opacity-50"
              >
                <RotateCcw className={`w-4 h-4 ${isUndoing ? 'animate-spin' : ''}`} />
                <span className="text-sm font-medium">
                  {isUndoing ? t('undoingAction') : t('undoBtn')}
                </span>
              </button>
              
              <button
                onClick={clearAction}
                disabled={isUndoing}
                className="p-1.5 text-zinc-400 hover:text-white transition-colors disabled:opacity-50"
              >
                <X className="w-4 h-4" />
              </button>
            </div>
          </div>

          {!isUndoing && (
            <motion.div
              key={lastAction.id}
              initial={{ width: '100%' }}
              animate={{ width: '0%' }}
              transition={{ duration: 10, ease: 'linear' }}
              className="h-1 bg-blue-500 dark:bg-blue-400 w-full"
            />
          )}
        </motion.div>
      )}
    </AnimatePresence>
  );
}

import React, { useState, useEffect } from 'react';
import { motion, AnimatePresence } from 'motion/react';
import { X, Heart, Star } from 'lucide-react';
import { useLanguage } from '../contexts/LanguageContext';
import { useActionHistory } from '../contexts/ActionHistoryContext';

export function NpsPopup() {
  const { t } = useLanguage();
  const { lastAction } = useActionHistory();
  const [isVisible, setIsVisible] = useState(false);
  const [hasShown, setHasShown] = useState(() => {
    return localStorage.getItem('nps_feedback_submitted') === 'true';
  });
  const [rating, setRating] = useState<number | null>(null);
  const [comment, setComment] = useState('');
  const [submitted, setSubmitted] = useState(false);

  // Trigger occasionally when the user performs an action
  useEffect(() => {
    if (lastAction && !hasShown && !isVisible) {
      // Small chance to show, or just wait a bit. For demo/preview let's make it reliable:
      // We will show it 5 seconds after an action is registered, if it hasn't shown yet.
      // But maybe let's add a random chance like Math.random() > 0.5. 
      // For testing, let's just show it on the first action after 8 seconds.
      const timer = setTimeout(() => {
        setIsVisible(true);
        setHasShown(true);
      }, 5000);
      return () => clearTimeout(timer);
    }
  }, [lastAction, hasShown, isVisible]);

  const handleSubmit = () => {
    if (rating === null) return;
    setSubmitted(true);
    localStorage.setItem('nps_feedback_submitted', 'true');
    setTimeout(() => {
      setIsVisible(false);
    }, 2500);
  };

  return (
    <AnimatePresence>
      {isVisible && (
        <motion.div
          initial={{ opacity: 0, y: 50, scale: 0.95 }}
          animate={{ opacity: 1, y: 0, scale: 1 }}
          exit={{ opacity: 0, scale: 0.95, y: 20 }}
          className="fixed bottom-6 left-6 z-50 w-[340px] bg-white dark:bg-zinc-900 border border-zinc-200 dark:border-zinc-800 rounded-2xl shadow-2xl p-5 flex flex-col gap-4"
        >
          {submitted ? (
            <div className="flex flex-col items-center justify-center py-6 gap-3 text-emerald-500 dark:text-emerald-400">
              <Heart className="w-8 h-8 animate-pulse fill-current" />
              <p className="font-medium text-center">{t('npsSuccess')}</p>
            </div>
          ) : (
            <>
              <div className="flex items-start justify-between">
                <div>
                  <h4 className="text-sm font-semibold text-zinc-900 dark:text-zinc-100">{t('npsTitle')}</h4>
                  <p className="text-xs text-zinc-500 dark:text-zinc-400 mt-1">{t('npsDesc')}</p>
                </div>
                <button 
                  onClick={() => setIsVisible(false)}
                  className="p-1 text-zinc-400 hover:text-zinc-600 dark:hover:text-zinc-200 transition-colors"
                >
                  <X className="w-4 h-4" />
                </button>
              </div>

              <div className="flex justify-between items-center gap-1">
                {[0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10].map((num) => (
                  <button
                    key={num}
                    onClick={() => setRating(num)}
                    className={`flex-1 aspect-square rounded-md text-[10px] font-medium transition-all flex items-center justify-center
                      ${rating === num 
                        ? 'bg-blue-600 text-white shadow-sm' 
                        : 'bg-zinc-100 dark:bg-zinc-800 text-zinc-600 dark:text-zinc-400 hover:bg-zinc-200 dark:hover:bg-zinc-700'
                      }`}
                  >
                    {num}
                  </button>
                ))}
              </div>

              <AnimatePresence>
                {rating !== null && (
                  <motion.div 
                    initial={{ opacity: 0, height: 0 }}
                    animate={{ opacity: 1, height: 'auto' }}
                    className="flex flex-col gap-3 overflow-hidden"
                  >
                    <textarea
                      value={comment}
                      onChange={(e) => setComment(e.target.value)}
                      placeholder={t('npsCommentPlaceholder')}
                      className="w-full text-xs p-3 rounded-xl bg-zinc-50 dark:bg-zinc-800/50 border border-zinc-200 dark:border-zinc-700 text-zinc-900 dark:text-zinc-100 focus:outline-none focus:ring-2 focus:ring-blue-500/50 resize-none h-20"
                    />
                    <button
                      onClick={handleSubmit}
                      className="w-full bg-zinc-900 dark:bg-zinc-100 text-white dark:text-zinc-900 py-2 rounded-xl text-xs font-semibold hover:bg-zinc-800 dark:hover:bg-zinc-200 transition-colors"
                    >
                      {t('npsSubmit')}
                    </button>
                  </motion.div>
                )}
              </AnimatePresence>
            </>
          )}
        </motion.div>
      )}
    </AnimatePresence>
  );
}

import React, { useState, useEffect } from 'react';
import { Loader2, Activity } from 'lucide-react';
import { useLanguage } from '../contexts/LanguageContext';
import { motion } from 'motion/react';

export function InitialScanner({ onComplete }: { onComplete: () => void }) {
  const { t } = useLanguage();
  const [progress, setProgress] = useState(0);
  const [step, setStep] = useState(1);

  useEffect(() => {
    const duration = 3000; // 3 seconds
    const interval = 30; // Update every 30ms
    const steps = duration / interval;
    let currentStep = 0;

    const timer = setInterval(() => {
      currentStep++;
      const newProgress = Math.min((currentStep / steps) * 100, 100);
      setProgress(newProgress);

      if (newProgress < 25) setStep(1);
      else if (newProgress < 50) setStep(2);
      else if (newProgress < 85) setStep(3);
      else setStep(4);

      if (newProgress >= 100) {
        clearInterval(timer);
        setTimeout(onComplete, 400); // Short delay before unmounting
      }
    }, interval);

    return () => clearInterval(timer);
  }, [onComplete]);

  const getStepText = () => {
    switch (step) {
      case 1: return t('scanStep1');
      case 2: return t('scanStep2');
      case 3: return t('scanStep3');
      case 4: return t('scanStep4');
      default: return t('scanStep1');
    }
  };

  return (
    <motion.div
      initial={{ opacity: 1 }}
      exit={{ opacity: 0, scale: 0.95 }}
      transition={{ duration: 0.4 }}
      className="fixed inset-0 bg-zinc-50 dark:bg-black flex flex-col items-center justify-center p-4 z-[100]"
    >
      <motion.div
        initial={{ opacity: 0, y: 20 }}
        animate={{ opacity: 1, y: 0 }}
        className="w-full max-w-md flex flex-col items-center"
      >
        <div className="w-20 h-20 bg-blue-100 dark:bg-blue-500/10 text-blue-600 dark:text-blue-500 rounded-3xl flex items-center justify-center mb-8 shadow-sm border border-blue-200 dark:border-blue-500/20 relative">
          <Activity className="w-10 h-10" />
          <div className="absolute inset-0 rounded-3xl border-2 border-blue-500/30 animate-ping" />
        </div>
        
        <h1 className="text-2xl font-bold text-zinc-900 dark:text-zinc-100 mb-2">
          {t('scanningTitle')}
        </h1>
        <p className="text-zinc-500 dark:text-zinc-400 text-center mb-10">
          {t('scanningDesc')}
        </p>

        <div className="w-full bg-zinc-200 dark:bg-zinc-800 rounded-full h-2 mb-4 overflow-hidden relative">
          <div
            className="bg-blue-600 dark:bg-blue-500 h-2 rounded-full transition-all duration-75 ease-linear absolute top-0 left-0"
            style={{ width: `${progress}%` }}
          />
        </div>

        <div className="flex items-center justify-between w-full text-sm font-medium">
          <span className="text-zinc-500 dark:text-zinc-400 flex items-center gap-2">
            <Loader2 className="w-4 h-4 animate-spin text-blue-500" />
            {getStepText()}
          </span>
          <span className="text-zinc-900 dark:text-zinc-100 font-bold">
            {Math.round(progress)}%
          </span>
        </div>
      </motion.div>
    </motion.div>
  );
}

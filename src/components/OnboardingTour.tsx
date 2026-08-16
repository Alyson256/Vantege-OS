import React, { useState } from 'react';
import { motion, AnimatePresence } from 'motion/react';
import { Rocket, LayoutDashboard, MonitorPlay, Settings2, X, ChevronRight, ChevronLeft } from 'lucide-react';
import { useLanguage } from '../contexts/LanguageContext';
import { cn } from '../lib/utils';

interface OnboardingTourProps {
  onComplete: () => void;
  onStepChange?: (stepId: string) => void;
}

export function OnboardingTour({ onComplete, onStepChange }: OnboardingTourProps) {
  const { t } = useLanguage();
  const [currentStep, setCurrentStep] = useState(0);

  const steps = [
    {
      id: 'welcome',
      icon: Rocket,
      title: t('tourWelcomeTitle'),
      desc: t('tourWelcomeDesc'),
      color: 'text-blue-500 dark:text-blue-400',
      bgColor: 'bg-blue-100 dark:bg-blue-500/20'
    },
    {
      id: 'dashboard',
      icon: LayoutDashboard,
      title: t('tourDashboardTitle'),
      desc: t('tourDashboardDesc'),
      color: 'text-indigo-500 dark:text-indigo-400',
      bgColor: 'bg-indigo-100 dark:bg-indigo-500/20'
    },
    {
      id: 'profiles',
      icon: MonitorPlay,
      title: t('tourProfilesTitle'),
      desc: t('tourProfilesDesc'),
      color: 'text-emerald-500 dark:text-emerald-400',
      bgColor: 'bg-emerald-100 dark:bg-emerald-500/20'
    },
    {
      id: 'custom',
      icon: Settings2,
      title: t('tourCustomTitle'),
      desc: t('tourCustomDesc'),
      color: 'text-purple-500 dark:text-purple-400',
      bgColor: 'bg-purple-100 dark:bg-purple-500/20'
    }
  ];

  // Notify parent of step change
  React.useEffect(() => {
    if (onStepChange) {
      onStepChange(steps[currentStep].id);
    }
  }, [currentStep, onStepChange]);

  const handleNext = () => {
    if (currentStep < steps.length - 1) {
      setCurrentStep(prev => prev + 1);
    } else {
      onComplete();
    }
  };

  const handlePrev = () => {
    if (currentStep > 0) {
      setCurrentStep(prev => prev - 1);
    }
  };

  return (
    <AnimatePresence>
      <div className="fixed inset-0 z-[100] flex items-center justify-center p-4">
        <motion.div 
          initial={{ opacity: 0 }} 
          animate={{ opacity: 1 }} 
          exit={{ opacity: 0 }} 
          className="absolute inset-0 bg-zinc-900/40 dark:bg-black/60 backdrop-blur-sm"
          onClick={onComplete}
        />
        
        <motion.div
          initial={{ opacity: 0, scale: 0.95, y: 20 }}
          animate={{ opacity: 1, scale: 1, y: 0 }}
          exit={{ opacity: 0, scale: 0.95, y: 20 }}
          className="relative w-full max-w-lg bg-white dark:bg-zinc-900 rounded-3xl shadow-2xl border border-zinc-200 dark:border-zinc-800 overflow-hidden"
          onClick={e => e.stopPropagation()}
        >
          {/* Close button */}
          <button 
            onClick={onComplete}
            className="absolute top-4 right-4 p-2 rounded-full text-zinc-400 hover:text-zinc-600 dark:hover:text-zinc-200 hover:bg-zinc-100 dark:hover:bg-zinc-800 transition-colors z-10"
          >
            <X className="w-5 h-5" />
          </button>

          {/* Content area */}
          <div className="p-8 pb-10">
            <AnimatePresence mode="wait">
              <motion.div
                key={currentStep}
                initial={{ opacity: 0, x: 20 }}
                animate={{ opacity: 1, x: 0 }}
                exit={{ opacity: 0, x: -20 }}
                transition={{ duration: 0.2 }}
                className="flex flex-col items-center text-center mt-4"
              >
                <div className={cn("w-20 h-20 rounded-2xl flex items-center justify-center mb-6", steps[currentStep].bgColor)}>
                  {React.createElement(steps[currentStep].icon, { className: cn("w-10 h-10", steps[currentStep].color) })}
                </div>
                
                <h2 className="text-2xl font-bold text-zinc-900 dark:text-zinc-50 mb-3">
                  {steps[currentStep].title}
                </h2>
                
                <p className="text-zinc-500 dark:text-zinc-400 leading-relaxed max-w-md">
                  {steps[currentStep].desc}
                </p>
              </motion.div>
            </AnimatePresence>
          </div>

          {/* Footer controls */}
          <div className="px-8 py-5 bg-zinc-50 dark:bg-zinc-800/50 border-t border-zinc-200 dark:border-zinc-800 flex items-center justify-between">
            {/* Dots */}
            <div className="flex items-center gap-2">
              {steps.map((_, idx) => (
                <div 
                  key={idx} 
                  className={cn(
                    "w-2 h-2 rounded-full transition-all duration-300",
                    idx === currentStep 
                      ? "bg-blue-600 dark:bg-blue-500 w-6" 
                      : "bg-zinc-300 dark:bg-zinc-700"
                  )}
                />
              ))}
            </div>

            {/* Buttons */}
            <div className="flex items-center gap-3">
              {currentStep > 0 && (
                <button 
                  onClick={handlePrev}
                  className="px-4 py-2 text-sm font-medium text-zinc-600 dark:text-zinc-400 hover:text-zinc-900 dark:hover:text-zinc-100 transition-colors"
                >
                  {t('tourPrev')}
                </button>
              )}
              
              <button 
                onClick={handleNext}
                className="flex items-center gap-1.5 px-5 py-2.5 rounded-xl bg-zinc-900 dark:bg-zinc-100 text-white dark:text-zinc-900 text-sm font-semibold hover:bg-zinc-800 dark:hover:bg-zinc-200 transition-colors shadow-sm"
              >
                {currentStep === steps.length - 1 ? t('tourStart') : (
                  <>
                    {t('tourNext')}
                    <ChevronRight className="w-4 h-4" />
                  </>
                )}
              </button>
            </div>
          </div>
        </motion.div>
      </div>
    </AnimatePresence>
  );
}

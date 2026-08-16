import React, { useState, useRef, useEffect } from 'react';
import { motion, AnimatePresence } from 'motion/react';
import { cn } from '../lib/utils';

interface TooltipProps {
  content: string;
  children: React.ReactNode;
  position?: 'top' | 'bottom' | 'left' | 'right';
  className?: string;
}

export function Tooltip({ content, children, position = 'top', className }: TooltipProps) {
  const [isVisible, setIsVisible] = useState(false);

  return (
    <div 
      className={cn("relative flex items-center justify-center group", className)}
      onMouseEnter={() => setIsVisible(true)}
      onMouseLeave={() => setIsVisible(false)}
      onFocus={() => setIsVisible(true)}
      onBlur={() => setIsVisible(false)}
    >
      {children}
      <AnimatePresence>
        {isVisible && (
          <motion.div
            initial={{ opacity: 0, scale: 0.95, y: position === 'top' ? 5 : position === 'bottom' ? -5 : 0, x: position === 'left' ? 5 : position === 'right' ? -5 : 0 }}
            animate={{ opacity: 1, scale: 1, y: 0, x: 0 }}
            exit={{ opacity: 0, scale: 0.95 }}
            transition={{ duration: 0.15, ease: "easeOut" }}
            className={cn(
              "absolute z-50 px-3 py-2 text-xs font-medium text-white bg-zinc-900 dark:bg-zinc-800 rounded-lg shadow-xl pointer-events-none w-max max-w-[250px] text-center leading-relaxed",
              position === 'top' && "bottom-full mb-2",
              position === 'bottom' && "top-full mt-2",
              position === 'left' && "right-full mr-2",
              position === 'right' && "left-full ml-2"
            )}
          >
            {content}
            <div className={cn(
              "absolute w-2 h-2 bg-zinc-900 dark:bg-zinc-800 transform rotate-45",
              position === 'top' && "bottom-[-4px] left-1/2 -translate-x-1/2",
              position === 'bottom' && "top-[-4px] left-1/2 -translate-x-1/2",
              position === 'left' && "right-[-4px] top-1/2 -translate-y-1/2",
              position === 'right' && "left-[-4px] top-1/2 -translate-y-1/2"
            )} />
          </motion.div>
        )}
      </AnimatePresence>
    </div>
  );
}

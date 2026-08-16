import React from 'react';
import { cn } from '../lib/utils';

interface LogoProps {
  className?: string;
}

export function Logo({ className }: LogoProps) {
  return (
    <svg 
      viewBox="0 0 100 100" 
      fill="none" 
      xmlns="http://www.w3.org/2000/svg" 
      className={cn("w-full h-full", className)}
    >
      <defs>
        <linearGradient id="neonBlue" x1="0%" y1="0%" x2="100%" y2="100%">
          <stop offset="0%" stopColor="#3b82f6" />
          <stop offset="100%" stopColor="#2563eb" />
        </linearGradient>
        <linearGradient id="neonCyan" x1="0%" y1="100%" x2="100%" y2="0%">
          <stop offset="0%" stopColor="#06b6d4" />
          <stop offset="100%" stopColor="#22d3ee" />
        </linearGradient>
        <linearGradient id="neonFire" x1="0%" y1="100%" x2="0%" y2="0%">
          <stop offset="0%" stopColor="#1d4ed8" />
          <stop offset="50%" stopColor="#3b82f6" />
          <stop offset="100%" stopColor="#60a5fa" />
        </linearGradient>
        <filter id="glow" x="-20%" y="-20%" width="140%" height="140%">
          <feGaussianBlur stdDeviation="3" result="blur" />
          <feComposite in="SourceGraphic" in2="blur" operator="over" />
        </filter>
        <filter id="glow-strong" x="-20%" y="-20%" width="140%" height="140%">
          <feGaussianBlur stdDeviation="4" result="blur" />
          <feComposite in="SourceGraphic" in2="blur" operator="over" />
        </filter>
      </defs>

      {/* Speedometer Outer Arc */}
      <path 
        d="M 15 75 A 45 45 0 1 1 85 75" 
        stroke="url(#neonBlue)" 
        strokeWidth="3" 
        strokeLinecap="round" 
        opacity="0.3"
      />
      
      {/* Speedometer Dash Arc */}
      <path 
        d="M 20 70 A 40 40 0 1 1 80 70" 
        stroke="url(#neonCyan)" 
        strokeWidth="5" 
        strokeLinecap="round" 
        strokeDasharray="2 8" 
        filter="url(#glow)"
      />

      {/* Speedometer Active Track (left to top-right) */}
      <path 
        d="M 20 70 A 40 40 0 0 1 75 35" 
        stroke="url(#neonBlue)" 
        strokeWidth="3" 
        strokeLinecap="round" 
        filter="url(#glow)"
      />

      {/* Windows 11 style 4 panes (Center) */}
      <g filter="url(#glow)">
        <rect x="38" y="42" width="10" height="10" rx="1.5" fill="url(#neonCyan)" />
        <rect x="52" y="42" width="10" height="10" rx="1.5" fill="url(#neonBlue)" />
        <rect x="38" y="56" width="10" height="10" rx="1.5" fill="url(#neonBlue)" />
        <rect x="52" y="56" width="10" height="10" rx="1.5" fill="url(#neonCyan)" />
      </g>

      {/* Speedometer Needle */}
      <g filter="url(#glow-strong)">
        <path 
          d="M 50 63 L 72 28" 
          stroke="#ffffff" 
          strokeWidth="2.5" 
          strokeLinecap="round" 
        />
        <circle cx="50" cy="63" r="3.5" fill="#ffffff" />
      </g>

      {/* Neon Fire Motif at the bottom */}
      <path 
        d="M 50 72 C 45 72 40 82 48 88 C 48 88 44 85 45 81 C 45 81 50 85 53 82 C 55 79 50 72 50 72 Z" 
        fill="url(#neonFire)" 
        filter="url(#glow)"
      />
    </svg>
  );
}

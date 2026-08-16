export type TweakCategory = 'Performance' | 'Privacy' | 'Network' | 'System' | 'UI';

export interface LocalizedString {
  pt: string;
  en: string;
}

export interface Tweak {
  id: string;
  title: LocalizedString;
  description: LocalizedString;
  registryDetails: LocalizedString;
  category: TweakCategory;
  recommendedFor: ('Gaming' | 'Checkup' | 'All')[];
}


import { acceptHMRUpdate, defineStore } from 'pinia';
import type { Observable } from 'rxjs';
import { of } from 'rxjs';
import { get } from '@vueuse/core';
import Log from 'consola';
import { tap } from 'rxjs/operators';
import type { ISetupResult, ILocaleConfig, I18nObjectType } from '@interfaces';
import { useSettingsStore } from './settingsStore';

export const useLocalizationStore = defineStore('LocalizationStore', () => {
	// State
	const state = reactive<{ i18nRef: I18nObjectType; locales: ILocaleConfig[] }>({
		i18nRef: {} as I18nObjectType,
		locales: [],
	});

	// Actions
	const actions = {
		setup(i18n?: I18nObjectType): Observable<ISetupResult> {
			return of({ name: useHelpStore.name, isSuccess: true }).pipe(tap(() => actions.setI18nObject(i18n)));
		},
		setI18nObject(i18n?: I18nObjectType) {
			if (!i18n) {
				Log.error('i18n object is not defined');
				return;
			}

			// @ts-expect-error - This is a valid assignment, typescript is being retarted here.
			state.i18nRef = i18n;
			state.locales = get(i18n.locales).map((x) => {
				return {
					text: x.text,
					code: x.code ?? '',
					iso: x.iso ?? '',
					bcp47Code: x.bcp47Code,
					file: x.file as string,
				};
			});
			Log.info('Localization Options:', get(getters.getLanguageLocaleOptions));
			actions.changeLanguageLocale(get(i18n.locale));
		},
		changeLanguageLocale(isoCode: string) {
			state.i18nRef.setLocale(isoCode).then(() => {
				useSettingsStore().languageSettings.language = isoCode;
				Log.info('Localization has been set to:', isoCode);
			});
		},
	};

	// Getters
	const getters = {
		getLanguageLocale: computed((): ILocaleConfig => {
			return state.locales.find((locale) => locale.code === state.i18nRef.locale) as ILocaleConfig;
		}),
		getLanguageLocaleOptions: computed((): ILocaleConfig[] => {
			return state.locales;
		}),
	};

	return {
		...toRefs(state),
		...actions,
		...getters,
	};
});

if (import.meta.hot) {
	import.meta.hot.accept(acceptHMRUpdate(useLocalizationStore, import.meta.hot));
}

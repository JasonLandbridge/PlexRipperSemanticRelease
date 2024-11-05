import { defineStore, acceptHMRUpdate } from 'pinia';
import { switchMap, tap } from 'rxjs/operators';
import type { Observable } from 'rxjs';
import { of } from 'rxjs';
import type { NotificationDTO } from '@dto';
import type { ISetupResult } from '@interfaces';
import { notificationApi } from '@api';

export const useNotificationsStore = defineStore('NotificationsStore', () => {
	const state = reactive<{ notifications: NotificationDTO[] }>({
		notifications: [],
	});

	const actions = {
		setup(): Observable<ISetupResult> {
			return actions.fetchNotifications().pipe(
				switchMap(() =>
					of({
						name: useNotificationsStore.name,
						isSuccess: true,
					}),
				),
			);
		},
		fetchNotifications() {
			return notificationApi.getAllNotificationsEndpoint().pipe(
				tap((result) => {
					if (result.isSuccess) {
						state.notifications = result.value ?? [];
					}
				}),
			);
		},
		setNotification(notification: NotificationDTO) {
			state.notifications.push(notification);
		},
		hideNotification(id: number): void {
			const i = state.notifications.findIndex((x) => x.id === id);
			if (i > -1) {
				state.notifications.splice(i, i, { ...state.notifications[i], hidden: true });
			}
			notificationApi
				.setNotificationVisibilityEndpoint({
					id,
					hidden: true,
				})
				.subscribe();
		},
		clearAllNotifications(): void {
			state.notifications = [];
			notificationApi.clearAllNotificationsEndpoint().subscribe();
		},
	};

	const getters = {
		getNotifications: computed((): NotificationDTO[] => state.notifications),
		getVisibleNotifications: computed((): NotificationDTO[] => state.notifications.filter((x) => !x.hidden)),
	};

	return {
		...toRefs(state),
		...actions,
		...getters,
	};
});

if (import.meta.hot) {
	import.meta.hot.accept(acceptHMRUpdate(useNotificationsStore, import.meta.hot));
}

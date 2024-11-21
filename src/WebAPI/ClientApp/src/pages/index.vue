<template>
	<QPage>
		<MediaOverview
			:library-id="0"
			all-media-mode
			:media-type="settingsStore.displaySettings.allOverviewViewMode" />
	</QPage>
</template>

<script setup lang="ts">
import { useSettingsStore } from '#imports';

const settingsStore = useSettingsStore();
</script>

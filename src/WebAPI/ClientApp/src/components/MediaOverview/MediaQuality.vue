<template>
	<div
		v-if="qualities.length"
		class="media-quality-container">
		<q-chip
			v-for="(quality, j) in qualities"
			:key="j"
			:color="getQualityColor(quality.quality)"
			size="md">
			{{ quality.displayQuality }}
		</q-chip>
	</div>
</template>

<script setup lang="ts">
import Log from 'consola';
import type { PlexMediaQualityDTO } from '@dto';

defineProps<{
	qualities: PlexMediaQualityDTO[];
}>();

const getQualityColor = (quality: string): string => {
	switch (quality) {
		case 'sd':
			return 'brown darken-4';
		case '480':
			return 'deep-orange';
		case '576':
			return 'yellow darken-1';
		case '720':
			return 'lime accent-4';
		case '1080':
			return 'blue accent-3';
		case '4k':
			return 'red darken-4';
		default:
			Log.debug('Missing quality color option', quality);
			return 'black';
	}
};
</script>

<style lang="scss">
.media-quality-container {
  padding: 0;
  text-align: center;
}
</style>

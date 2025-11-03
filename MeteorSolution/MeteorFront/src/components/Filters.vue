<template>
  <div class="filters">
    <select v-model="filters.YearFrom">
      <option value="">Год с...</option>
      <option v-for="year in years" :key="year" :value="year">{{ year }}</option>
    </select>

    <select v-model="filters.YearTo">
      <option value="">Год по...</option>
      <option v-for="year in years" :key="year" :value="year">{{ year }}</option>
    </select>

    <select v-model="filters.RecclassId">
      <option value="">Класс метеорита...</option>
      <option v-for="opt in recclassOptions" :key="opt.id" :value="opt.id">{{ opt.name }}</option>
    </select>

    <input
      type="text"
      v-model="filters.NameContains"
      placeholder="Часть названия..."
    />

    <select v-model="filters.SortBy">
      <option value="year">Год</option>
      <option value="count">Количество</option>
      <option value="totalMass">Суммарная масса</option>
    </select>

    <label>
      <input type="checkbox" v-model="filters.Desc" /> По убыванию
    </label>

    <button @click="$emit('apply', { ...filters })">Применить</button>
  </div>
</template>

<script setup>
import { reactive, ref, onMounted } from "vue";
import { getMeteoriteTypes } from "../api/meteorites";

const filters = reactive({
  YearFrom: "",
  YearTo: "",
  RecclassId: "",
  NameContains: "",
  SortBy: "year",
  Desc: false,
});

const recclassOptions = ref([]);

onMounted(async () => {
  try {
    const types = await getMeteoriteTypes();
    recclassOptions.value = (types || []).map(t => ({ id: t.id ?? t.Id, name: t.name ?? t.Name }));
  } catch (e) {
    console.error('Failed to load meteorite types', e);
  }
});

const years = Array.from({ length: 2025 - 1800 }, (_, i) => 1800 + i);
</script>

<style scoped>
.filters {
  display: flex;
  gap: 1rem;
  margin-bottom: 1rem;
}
</style>
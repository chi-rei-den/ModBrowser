'use strict';

var gulp = require('gulp'),
    concat = require('gulp-concat'),
    cssmin = require('gulp-cssmin'),
    uglify = require('gulp-uglify'),
    merge = require('merge-stream'),
    del = require('del'),
    bundleconfig = require('./bundleconfig.json');

const regex = {
    css: /\.css$/,
    js: /\.js$/
};

var webroot = './wwwroot/';

var paths = {
    libs: webroot + 'lib/'
};

gulp.task('clean:libs', () => {
    return del(paths.libs);
});

gulp.task('clean:outputFile', () => {
    return del(bundleconfig.map(bundle => bundle.outputFileName));
});


gulp.task('clean', gulp.series(['clean:libs','clean:outputFile']));

const getBundles = (regexPattern) => {
    return bundleconfig.filter(bundle => {
        return regexPattern.test(bundle.outputFileName);
    });
};

gulp.task('min:js', async function () {
    merge(getBundles(regex.js).map(bundle => {
        return gulp.src(bundle.inputFiles, { base: '.' })
            .pipe(concat(bundle.outputFileName))
            .pipe(uglify())
            .pipe(gulp.dest('.'));
    }))
});

gulp.task('min:css', async function () {
    merge(getBundles(regex.css).map(bundle => {
        return gulp.src(bundle.inputFiles, { base: '.' })
            .pipe(concat(bundle.outputFileName))
            .pipe(cssmin())
            .pipe(gulp.dest('.'));
    }))
});

gulp.task('min', gulp.series(['min:js', 'min:css']));

gulp.task('libs', ()=> {
    return gulp.src([
        'bootstrap/dist/js/*.min.js',
        'bootstrap/dist/js/*.min.js.map',
        'bootstrap/dist/css/*.min.css',
        'bootstrap/dist/css/*.min.css.map',
        'jquery/dist/jquery.min.js',
        'jquery/dist/jquery.min.map',
        'jquery-validation/dist/*.min.js',
        'jquery-validation-unobtrusive/dist/*.min.js',
        'masonry-layout/dist/*.min.js'
    ], {
        cwd: 'node_modules/**'
    }).pipe(gulp.dest(paths.libs));
});

gulp.task('default', gulp.series(['clean', 'min', 'libs']));